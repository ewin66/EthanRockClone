//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the T4\Model.tt template.
//
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace Rock.REST.CMS
{
	/// <summary>
	/// REST WCF service for BlogTags
	/// </summary>
    [Export(typeof(IService))]
    [ExportMetadata("RouteName", "CMS/BlogTag")]
	[AspNetCompatibilityRequirements( RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed )]
    public partial class BlogTagService : IBlogTagService, IService
    {
		/// <summary>
		/// Gets a BlogTag object
		/// </summary>
		[WebGet( UriTemplate = "{id}" )]
        public Rock.CMS.DTO.BlogTag Get( string id )
        {
            var currentUser = Rock.CMS.UserService.GetCurrentUser();
            if ( currentUser == null )
                throw new WebFaultException<string>("Must be logged in", System.Net.HttpStatusCode.Forbidden );

            using (Rock.Data.UnitOfWorkScope uow = new Rock.Data.UnitOfWorkScope())
            {
				uow.objectContext.Configuration.ProxyCreationEnabled = false;
				Rock.CMS.BlogTagService BlogTagService = new Rock.CMS.BlogTagService();
				Rock.CMS.BlogTag BlogTag = BlogTagService.Get( int.Parse( id ) );
				if ( BlogTag.Authorized( "View", currentUser ) )
					return BlogTag.DataTransferObject;
				else
					throw new WebFaultException<string>( "Not Authorized to View this BlogTag", System.Net.HttpStatusCode.Forbidden );
            }
        }
		
		/// <summary>
		/// Gets a BlogTag object
		/// </summary>
		[WebGet( UriTemplate = "{id}/{apiKey}" )]
        public Rock.CMS.DTO.BlogTag ApiGet( string id, string apiKey )
        {
            using (Rock.Data.UnitOfWorkScope uow = new Rock.Data.UnitOfWorkScope())
            {
				Rock.CMS.UserService userService = new Rock.CMS.UserService();
                Rock.CMS.User user = userService.Queryable().Where( u => u.ApiKey == apiKey ).FirstOrDefault();

				if (user != null)
				{
					uow.objectContext.Configuration.ProxyCreationEnabled = false;
					Rock.CMS.BlogTagService BlogTagService = new Rock.CMS.BlogTagService();
					Rock.CMS.BlogTag BlogTag = BlogTagService.Get( int.Parse( id ) );
					if ( BlogTag.Authorized( "View", user ) )
						return BlogTag.DataTransferObject;
					else
						throw new WebFaultException<string>( "Not Authorized to View this BlogTag", System.Net.HttpStatusCode.Forbidden );
				}
				else
					throw new WebFaultException<string>( "Invalid API Key", System.Net.HttpStatusCode.Forbidden );
            }
        }
		
		/// <summary>
		/// Updates a BlogTag object
		/// </summary>
		[WebInvoke( Method = "PUT", UriTemplate = "{id}" )]
        public void UpdateBlogTag( string id, Rock.CMS.DTO.BlogTag BlogTag )
        {
            var currentUser = Rock.CMS.UserService.GetCurrentUser();
            if ( currentUser == null )
                throw new WebFaultException<string>("Must be logged in", System.Net.HttpStatusCode.Forbidden );

            using ( Rock.Data.UnitOfWorkScope uow = new Rock.Data.UnitOfWorkScope() )
            {
				uow.objectContext.Configuration.ProxyCreationEnabled = false;
				Rock.CMS.BlogTagService BlogTagService = new Rock.CMS.BlogTagService();
				Rock.CMS.BlogTag existingBlogTag = BlogTagService.Get( int.Parse( id ) );
				if ( existingBlogTag.Authorized( "Edit", currentUser ) )
				{
					uow.objectContext.Entry(existingBlogTag).CurrentValues.SetValues(BlogTag);
					
					if (existingBlogTag.IsValid)
						BlogTagService.Save( existingBlogTag, currentUser.PersonId );
					else
						throw new WebFaultException<string>( existingBlogTag.ValidationResults.AsDelimited(", "), System.Net.HttpStatusCode.BadRequest );
				}
				else
					throw new WebFaultException<string>( "Not Authorized to Edit this BlogTag", System.Net.HttpStatusCode.Forbidden );
            }
        }

		/// <summary>
		/// Updates a BlogTag object
		/// </summary>
		[WebInvoke( Method = "PUT", UriTemplate = "{id}/{apiKey}" )]
        public void ApiUpdateBlogTag( string id, string apiKey, Rock.CMS.DTO.BlogTag BlogTag )
        {
            using ( Rock.Data.UnitOfWorkScope uow = new Rock.Data.UnitOfWorkScope() )
            {
				Rock.CMS.UserService userService = new Rock.CMS.UserService();
                Rock.CMS.User user = userService.Queryable().Where( u => u.ApiKey == apiKey ).FirstOrDefault();

				if (user != null)
				{
					uow.objectContext.Configuration.ProxyCreationEnabled = false;
					Rock.CMS.BlogTagService BlogTagService = new Rock.CMS.BlogTagService();
					Rock.CMS.BlogTag existingBlogTag = BlogTagService.Get( int.Parse( id ) );
					if ( existingBlogTag.Authorized( "Edit", user ) )
					{
						uow.objectContext.Entry(existingBlogTag).CurrentValues.SetValues(BlogTag);
					
						if (existingBlogTag.IsValid)
							BlogTagService.Save( existingBlogTag, user.PersonId );
						else
							throw new WebFaultException<string>( existingBlogTag.ValidationResults.AsDelimited(", "), System.Net.HttpStatusCode.BadRequest );
					}
					else
						throw new WebFaultException<string>( "Not Authorized to Edit this BlogTag", System.Net.HttpStatusCode.Forbidden );
				}
				else
					throw new WebFaultException<string>( "Invalid API Key", System.Net.HttpStatusCode.Forbidden );
            }
        }

		/// <summary>
		/// Creates a new BlogTag object
		/// </summary>
		[WebInvoke( Method = "POST", UriTemplate = "" )]
        public void CreateBlogTag( Rock.CMS.DTO.BlogTag BlogTag )
        {
            var currentUser = Rock.CMS.UserService.GetCurrentUser();
            if ( currentUser == null )
                throw new WebFaultException<string>("Must be logged in", System.Net.HttpStatusCode.Forbidden );

            using ( Rock.Data.UnitOfWorkScope uow = new Rock.Data.UnitOfWorkScope() )
            {
				uow.objectContext.Configuration.ProxyCreationEnabled = false;
				Rock.CMS.BlogTagService BlogTagService = new Rock.CMS.BlogTagService();
				Rock.CMS.BlogTag existingBlogTag = new Rock.CMS.BlogTag();
				BlogTagService.Add( existingBlogTag, currentUser.PersonId );
				uow.objectContext.Entry(existingBlogTag).CurrentValues.SetValues(BlogTag);

				if (existingBlogTag.IsValid)
					BlogTagService.Save( existingBlogTag, currentUser.PersonId );
				else
					throw new WebFaultException<string>( existingBlogTag.ValidationResults.AsDelimited(", "), System.Net.HttpStatusCode.BadRequest );
            }
        }

		/// <summary>
		/// Creates a new BlogTag object
		/// </summary>
		[WebInvoke( Method = "POST", UriTemplate = "{apiKey}" )]
        public void ApiCreateBlogTag( string apiKey, Rock.CMS.DTO.BlogTag BlogTag )
        {
            using ( Rock.Data.UnitOfWorkScope uow = new Rock.Data.UnitOfWorkScope() )
            {
				Rock.CMS.UserService userService = new Rock.CMS.UserService();
                Rock.CMS.User user = userService.Queryable().Where( u => u.ApiKey == apiKey ).FirstOrDefault();

				if (user != null)
				{
					uow.objectContext.Configuration.ProxyCreationEnabled = false;
					Rock.CMS.BlogTagService BlogTagService = new Rock.CMS.BlogTagService();
					Rock.CMS.BlogTag existingBlogTag = new Rock.CMS.BlogTag();
					BlogTagService.Add( existingBlogTag, user.PersonId );
					uow.objectContext.Entry(existingBlogTag).CurrentValues.SetValues(BlogTag);

					if (existingBlogTag.IsValid)
						BlogTagService.Save( existingBlogTag, user.PersonId );
					else
						throw new WebFaultException<string>( existingBlogTag.ValidationResults.AsDelimited(", "), System.Net.HttpStatusCode.BadRequest );
				}
				else
					throw new WebFaultException<string>( "Invalid API Key", System.Net.HttpStatusCode.Forbidden );
            }
        }

		/// <summary>
		/// Deletes a BlogTag object
		/// </summary>
		[WebInvoke( Method = "DELETE", UriTemplate = "{id}" )]
        public void DeleteBlogTag( string id )
        {
            var currentUser = Rock.CMS.UserService.GetCurrentUser();
            if ( currentUser == null )
                throw new WebFaultException<string>("Must be logged in", System.Net.HttpStatusCode.Forbidden );

            using ( Rock.Data.UnitOfWorkScope uow = new Rock.Data.UnitOfWorkScope() )
            {
				uow.objectContext.Configuration.ProxyCreationEnabled = false;
				Rock.CMS.BlogTagService BlogTagService = new Rock.CMS.BlogTagService();
				Rock.CMS.BlogTag BlogTag = BlogTagService.Get( int.Parse( id ) );
				if ( BlogTag.Authorized( "Edit", currentUser ) )
				{
					BlogTagService.Delete( BlogTag, currentUser.PersonId );
					BlogTagService.Save( BlogTag, currentUser.PersonId );
				}
				else
					throw new WebFaultException<string>( "Not Authorized to Edit this BlogTag", System.Net.HttpStatusCode.Forbidden );
            }
        }

		/// <summary>
		/// Deletes a BlogTag object
		/// </summary>
		[WebInvoke( Method = "DELETE", UriTemplate = "{id}/{apiKey}" )]
        public void ApiDeleteBlogTag( string id, string apiKey )
        {
            using ( Rock.Data.UnitOfWorkScope uow = new Rock.Data.UnitOfWorkScope() )
            {
				Rock.CMS.UserService userService = new Rock.CMS.UserService();
                Rock.CMS.User user = userService.Queryable().Where( u => u.ApiKey == apiKey ).FirstOrDefault();

				if (user != null)
				{
					uow.objectContext.Configuration.ProxyCreationEnabled = false;
					Rock.CMS.BlogTagService BlogTagService = new Rock.CMS.BlogTagService();
					Rock.CMS.BlogTag BlogTag = BlogTagService.Get( int.Parse( id ) );
					if ( BlogTag.Authorized( "Edit", user ) )
					{
						BlogTagService.Delete( BlogTag, user.PersonId );
						BlogTagService.Save( BlogTag, user.PersonId );
					}
					else
						throw new WebFaultException<string>( "Not Authorized to Edit this BlogTag", System.Net.HttpStatusCode.Forbidden );
				}
				else
					throw new WebFaultException<string>( "Invalid API Key", System.Net.HttpStatusCode.Forbidden );
            }
        }

    }
}
