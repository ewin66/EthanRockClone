﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Rock.BulkImport
{
    /// <summary>
    /// Import record from ~/api/Interaction/Import
    /// </summary>
    public class InteractionImport
    {
        /// <summary>
        /// Gets or sets the interaction channel identifier.
        /// If this isn't specified, <see cref="InteractionChannelGuid"/> or <see cref="InteractionChannelForeignKey"/> must be specified
        /// </summary>
        /// <value>
        /// The interaction channel identifier.
        /// </value>
        public int? InteractionChannelId { get; set; }

        /// <summary>
        /// Gets or sets the interaction channel unique identifier.
        /// If this isn't specified, <see cref="InteractionChannelId"/> or <see cref="InteractionChannelForeignKey"/> must be specified
        /// </summary>
        /// <value>
        /// The interaction channel unique identifier.
        /// </value>
        public Guid? InteractionChannelGuid { get; set; }

        /// <summary>
        /// Gets or sets the interaction channel foreign key.
        /// If this isn't specified, <see cref="InteractionChannelId"/> or <see cref="InteractionChannelGuid"/> must be specified
        /// </summary>
        /// <value>
        /// The interaction channel foreign key.
        /// </value>
        public string InteractionChannelForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the interaction channel.
        /// </summary>
        /// <value>
        /// The name of the interaction channel.
        /// </value>
        public string InteractionChannelName { get; set; }

        /// <summary>
        /// Gets or sets the interaction component identifier.
        /// If this isn't specified, <see cref="InteractionComponentGuid"/> or <see cref="InteractionComponentForeignKey"/> must be specified
        /// </summary>
        /// <value>
        /// The interaction component identifier.
        /// </value>
        public int? InteractionComponentId { get; set; }

        /// <summary>
        /// Gets or sets the interaction component unique identifier.
        /// If this isn't specified, <see cref="InteractionComponentId"/> or <see cref="InteractionComponentForeignKey"/> must be specified
        /// </summary>
        /// <value>
        /// The interaction component unique identifier.
        /// </value>
        public Guid? InteractionComponentGuid { get; set; }

        /// <summary>
        /// Gets or sets the interaction component foreign key.
        /// If this isn't specified, <see cref="InteractionComponentGuid"/> or <see cref="InteractionComponentId"/> must be specified
        /// </summary>
        /// <value>
        /// The interaction component foreign key.
        /// </value>
        public string InteractionComponentForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the interaction component.
        /// </summary>
        /// <value>
        /// The name of the interaction component.
        /// </value>
        public string InteractionComponentName { get; set; }

        /// <summary>
        /// Gets or sets the interaction.
        /// </summary>
        /// <value>
        /// The interaction.
        /// </value>
        public InteractionImportInteraction Interaction { get; set; }
    }


    /// <summary>
    /// 
    /// </summary>
    public class InteractionImportInteraction
    {
        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        [StringLength( 25 )]
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the interaction date time.
        /// </summary>
        /// <value>
        /// The interaction date time.
        /// </value>
        public DateTime InteractionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the interaction end date time.
        /// </summary>
        /// <value>
        /// The interaction end date time.
        /// </value>
        public DateTime? InteractionEndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity that this interaction component is related to.
        /// For example:
        ///  if this is a Page View:
        ///     Interaction.EntityId is the Page.Id of the page that was viewed
        ///  if this is a Communication Recipient activity:
        ///     Interaction.EntityId is the CommunicationRecipient.Id that did the click or open
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the entity (object) that this interaction component is related to.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the interaction summary.
        /// </summary>
        /// <value>
        /// The interaction summary.
        /// </value>
        public string InteractionSummary { get; set; }

        /// <summary>
        /// Gets or sets the interaction data.
        /// </summary>
        /// <value>
        /// The interaction data.
        /// </value>
        public string InteractionData { get; set; }

        /// <summary>
        /// Gets or sets the personal device identifier.
        /// </summary>
        /// <value>
        /// The personal device identifier.
        /// </value>
        public int? PersonalDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the related entity type identifier.
        /// </summary>
        /// <value>
        /// The related entity type identifier.
        /// </value>
        public int? RelatedEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the related entity identifier.
        /// </summary>
        /// <value>
        /// The related entity identifier.
        /// </value>
        public int? RelatedEntityId { get; set; }

        #region Campaign Meta fields

        /// <summary>
        /// Gets or sets the campaign source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the campaign medium.
        /// </summary>
        /// <value>
        /// The medium.
        /// </value>
        public string Medium { get; set; }

        /// <summary>
        /// Gets or sets the campaign name
        /// </summary>
        /// <value>
        /// The campaign.
        /// </value>
        public string Campaign { get; set; }

        /// <summary>
        /// Gets or sets the campaign content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the campaign term(s).
        /// </summary>
        /// <value>
        /// The term.
        /// </value>
        public string Term { get; set; }

        #endregion Campaign Meta fields

        #region Fields that were introduced in v11.0

        /// <summary>
        /// Gets or sets the channel custom1.
        /// </summary>
        /// <value>
        /// The channel custom1.
        /// </value>
        public string ChannelCustom1 { get; set; }

        /// <summary>
        /// Gets or sets the channel custom2.
        /// </summary>
        /// <value>
        /// The channel custom2.
        /// </value>
        public string ChannelCustom2 { get; set; }

        /// <summary>
        /// Gets or sets the channel custom indexed1.
        /// </summary>
        /// <value>
        /// The channel custom indexed1.
        /// </value>
        public string ChannelCustomIndexed1 { get; set; }

        // TODO, what is this??
        public double? InteractionLength { get; set; }

        // TODO, what unit of time is this??? Seconds??
        public double? InteractionTimeToServe { get; set; }

        #endregion Fields that were introduced in v11.0

        /// <summary>
        /// Gets or sets the foreign identifier.
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        public int ForeignId { get; set; }

        /// <summary>
        /// Gets or sets the foreign key.
        /// </summary>
        /// <value>
        /// The foreign key.
        /// </value>
        public string ForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the foreign unique identifier.
        /// </summary>
        /// <value>
        /// The foreign unique identifier.
        /// </value>
        public Guid? ForeignGuid { get; set; }
    }

}
