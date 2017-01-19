﻿using System;
using System.Linq;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [BooleanField( "Delete Job if no data left to migrate", "Determines if this Job will delete itself if there is no data left to migrate to the Interactions table.", true, key: "DeleteJob" )]
    public class MigrateInteractionsData : IJob
    {
        private int _channelsInserted = 0;
        private int _componentsInserted = 0;
        private int _deviceTypesInserted = 0;
        private int _sessionsInserted = 0;
        private int _pageViewsMoved = 0;
        private int _pageViewsTotal = 0;
        private int _communicationRecipientActivityMoved = 0;
        private int _communicationRecipientActivityTotal = 0;

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>

        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var deleteJob = dataMap.Get( "DeleteJob" ).ToStringSafe().AsBoolean();

            using ( var rockContext = new RockContext() )
            {
                _pageViewsTotal = rockContext.Database.SqlQuery<int>( "SELECT COUNT(*) FROM PageView" ).First();
                _communicationRecipientActivityTotal = rockContext.Database.SqlQuery<int>( "SELECT COUNT(*) FROM CommunicationRecipientActivity" ).First();

                if ( _pageViewsTotal == 0 && _communicationRecipientActivityTotal == 0 && deleteJob )
                {
                    // delete job if there are no PageView or CommunicationRecipientActivity rows  left
                    var jobId = context.GetJobId();
                    var jobService = new ServiceJobService( rockContext );
                    var job = jobService.Get( jobId );
                    if ( job != null )
                    {
                        jobService.Delete( job );
                        rockContext.SaveChanges();
                        return;
                    }
                }
            }

            MigratePageViewsData( context );

            context.UpdateLastStatusMessage( $@"Channels Inserted: {_channelsInserted}, 
Components Inserted: {_componentsInserted}, 
DeviceTypes Inserted: {_deviceTypesInserted},
Sessions Inserted: {_sessionsInserted},
PageViews Moved: {_pageViewsMoved}/{_pageViewsTotal},
CommunicationRecipientActivity Moved: {_communicationRecipientActivityMoved}/{_communicationRecipientActivityTotal}
" );
        }

        /// <summary>
        /// Migrates the page views data.
        /// </summary>
        /// <param name="context">The context.</param>
        private void MigratePageViewsData( IJobExecutionContext context )
        {
            using ( var rockContext = new RockContext() )
            {
                var componentEntityTypePage = EntityTypeCache.Read<Rock.Model.Page>();
                var channelMediumWebsite = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE );
                var sqlInsertSitesToChannels = $@"
-- Insert Websites
INSERT INTO [InteractionChannel] (
    [Name]
    ,[ComponentEntityTypeId]
    ,[ChannelTypeMediumValueId]
    ,[ChannelEntityId]
    ,[Guid]
    )
SELECT s.[Name] [Site.Name]
    ,{componentEntityTypePage.Id}
    ,'{channelMediumWebsite.Id}'
    ,s.[Id] [SiteId]
    ,NEWID() AS NewGuid
FROM [PageView] pv
INNER JOIN [Site] s ON pv.[SiteId] = s.[Id]
WHERE s.Id NOT IN (
        SELECT ChannelEntityId
        FROM InteractionChannel where ChannelEntityId is not null
        )
GROUP BY s.[Id]
    ,s.[Name]";

                var sqlInsertPagesToComponents = @"
INSERT INTO [InteractionComponent] (
    [Name]
    ,[EntityId]
    ,[Guid]
    ,[ChannelId]
    )
SELECT isnull(pv.[PageTitle], '')
    ,pv.[PageId]
    ,NEWID() AS NewGuid
    ,c.[Id]
FROM [PageView] pv 
INNER JOIN [Site] s ON pv.SiteId = s.Id
INNER JOIN [InteractionChannel] c ON s.[Id] = c.[ChannelEntityId]
AND CONCAT (
        pv.[PageTitle]
		,'_'
		,pv.PageId
        ,'_'
        ,c.Id
        ) NOT IN (
        SELECT CONCAT (
				[Name]
				,'_'
                ,EntityId
                ,'_'
                ,ChannelId
                )
        FROM InteractionComponent
        )
GROUP BY pv.[PageId]
    ,isnull(pv.[PageTitle], '')
    ,c.[Id]
";

                var insertUserAgentToDeviceTypes = @"
-- Insert Devices
INSERT INTO [InteractionDeviceType] (
    [Name]
    ,[Application]
    ,[DeviceTypeData]
    ,[ClientType]
    ,[OperatingSystem]
    ,[Guid]
    ,[ForeignId]
    )
SELECT [OperatingSystem] + ' - ' + [Browser]
    ,[Browser]
    ,[UserAgent]
    ,[ClientType]
    ,[OperatingSystem]
    ,NEWID()
    ,[Id]
FROM [PageViewUserAgent]
WHERE Id NOT IN (
        SELECT ForeignId
        FROM InteractionDeviceType where ForeignId is not null
        )
";

                var insertSessions = @"
INSERT INTO [InteractionSession] (
    [DeviceTypeId]
    ,[IpAddress]
    ,[Guid]
    ,[ForeignId]
    )
SELECT c.[Id]
    ,a.[IpAddress]
    ,a.[Guid]
    ,a.[Id]
FROM [PageViewSession] a
INNER JOIN [PageViewUserAgent] AS b ON a.[PageViewUserAgentId] = b.[Id]
INNER JOIN [InteractionDeviceType] AS c ON c.[ForeignId] = a.[PageViewUserAgentId]
WHERE a.Id NOT IN (
        SELECT ForeignId
        FROM InteractionSession where ForeignId is not null
        );";

                _channelsInserted += rockContext.Database.ExecuteSqlCommand( sqlInsertSitesToChannels );
                _componentsInserted = rockContext.Database.ExecuteSqlCommand( sqlInsertPagesToComponents );
                _deviceTypesInserted = rockContext.Database.ExecuteSqlCommand( insertUserAgentToDeviceTypes );
                _sessionsInserted = rockContext.Database.ExecuteSqlCommand( insertSessions );
                var interactionService = new InteractionService( rockContext );

                // move PageView data in chunks (see http://dba.stackexchange.com/questions/1750/methods-of-speeding-up-a-huge-delete-from-table-with-no-clauses)
                bool keepMoving = true;

                while ( keepMoving )
                {
                    var dbTransaction = rockContext.Database.BeginTransaction();
                    try
                    {
                        // keep track of where to start so that SQL doesn't have to scan the whole table when deleting
                        int insertStartId = interactionService.Queryable().Max( a => a.Id );
                        int chunkSize = 25000;

                        int rowsMoved = rockContext.Database.ExecuteSqlCommand( $@"
                            INSERT INTO [Interaction]  WITH (TABLOCK) (
		                        [InteractionDateTime]
		                        ,[Operation]
		                        ,[InteractionComponentId]
		                        ,[InteractionSessionId]
		                        ,[InteractionData]
		                        ,[PersonAliasId]
		                        ,[Guid]
		                        )
	                        SELECT TOP ({chunkSize}) *
	                        FROM (
		                        SELECT [DateTimeViewed]
			                        ,'View' [Operation]
			                        ,cmp.[Id] [InteractionComponentId]
			                        ,s.[Id] [InteractionSessionId]
			                        ,pv.[Url] [InteractionData]
			                        ,pv.[PersonAliasId]
			                        ,pv.[Guid] [Guid]
		                        FROM [PageView] pv
		                        CROSS APPLY (
			                        SELECT max(id) [Id]
			                        FROM [InteractionComponent] cmp
			                        WHERE ISNULL(pv.[PageId], 0) = ISNULL(cmp.[EntityId], 0)
				                        AND isnull(pv.[PageTitle], '') = isnull(cmp.[Name], '')
			                        ) cmp
		                        CROSS APPLY (
			                        SELECT top 1 s.Id [Id]
			                        FROM [InteractionSession] s
			                        WHERE s.[ForeignId] = pv.[PageViewSessionId]
				                        AND s.ForeignId IS NOT NULL
			                        ) s
		                        where cmp.Id is not null
		                        ) x " );

                        // delete PageViews that have been moved to the Interaction table
                        rockContext.Database.ExecuteSqlCommand( $"delete from PageView with (tablock) where [Guid] in (select [Guid] from Interaction WHERE Id >= {insertStartId})" );

                        keepMoving = rowsMoved > 0;
                        _pageViewsMoved += rowsMoved;
                    }
                    finally
                    {
                        dbTransaction.Commit();
                    }

                    var percentComplete = ( _pageViewsMoved * 100.0 ) / _pageViewsTotal;
                    var statusMessage = $@"Progress: {_pageViewsMoved} of {_pageViewsTotal} ({Math.Round( percentComplete, 1 )}%) PageViews data migrated to Interactions";
                    context.UpdateLastStatusMessage( statusMessage );
                }
            }
        }
    }
}
