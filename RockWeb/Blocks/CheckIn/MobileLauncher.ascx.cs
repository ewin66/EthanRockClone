﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Mobile Launcher" )]
    [Category( "Check-in" )]
    [Description( "Launch page for checking in from a person's mobile device." )]

    #region Block Attributes

    [TextField(
        "Devices",
        Key = AttributeKey.DeviceIdList,
        Category = "CustomSetting",
        Description = "The devices to consider for determining the kiosk. No value would consider all devices in the system. If none are selected, then use all devices.",
        IsRequired = false,
        Order = 1 )]

    [TextField(
        "Check-in Theme",
        Key = AttributeKey.CheckinTheme,
        Category = "CustomSetting",
        IsRequired = true,
        Description = "The check-in theme to pass to the check-in pages.",
        Order = 2
        )]

    [TextField(
        "Check-in Configuration",
        Key = AttributeKey.CheckinConfiguration_GroupTypeId,
        Category = "CustomSetting",
        IsRequired = true,
        Description = "The check-in configuration to use." )]

    [TextField(
        "Check-in Areas",
        Key = AttributeKey.ConfiguredAreas_GroupTypeIds,
        Category = "CustomSetting",
        IsRequired = true,
        Description = "The check-in areas to use." )]

    #endregion Block Attributes


    #region Block Attributes for Text options

    [TextField(
        "Identify you Prompt Template <span class='tip tip-lava'></span>",
        Key = AttributeKey.IdentifyYouPromptTemplate,
        Category = "CustomSetting",
        DefaultValue = "Before we proceed we'll need you to identify you for check-in.",
        IsRequired = true,
        Description = "" )]

    [TextField(
        "Allow Location Prompt <span class='tip tip-lava'></span>",
        Key = AttributeKey.AllowLocationPermissionPromptTemplate,
        Category = "CustomSetting",
        DefaultValue = "We need to determine your location to complete the check-in process. You'll notice a request window pop-up. Be sure to allow permissions. We'll only have permission to you location when you're visiting this site.",
        IsRequired = true,
        Description = "" )]

    [TextField(
        "LocationProgress",
        Key = AttributeKey.LocationProgress,
        Category = "CustomSetting",
        DefaultValue = "Determining location...",
        IsRequired = true,
        Description = "" )]

    [TextField(
        "Welcome Back <span class='tip tip-lava'></span>",
        Key = AttributeKey.WelcomeBackTemplate,
        Category = "CustomSetting",
        DefaultValue = "Hi {{ CurrentPerson.NickName }}! Great to see to see you back. Select the check-in button to get started.",
        IsRequired = true,
        Description = "" )]

    [TextField(
        "No Services <span class='tip tip-lava'></span>",
        Key = AttributeKey.NoScheduledDevicesAvailableTemplate,
        Category = "CustomSetting",
        DefaultValue = "Hi {{ CurrentPerson.NickName }}! There are currently no services ready for check-in at this time.",
        IsRequired = true,
        Description = "" )]

    [TextField(
        "Can't determine location. <span class='tip tip-lava'></span>",
        Key = AttributeKey.UnableToDetermineMobileLocationTemplate,
        Category = "CustomSetting",
        DefaultValue = "Hi {{ CurrentPerson.NickName }}! We can't determine your location. Please be sure to enable location permissions for your device.",
        IsRequired = true,
        Description = "" )]

    [TextField(
        "No Devices Found <span class='tip tip-lava'></span>",
        Key = AttributeKey.NoDevicesFoundTemplate,
        Category = "CustomSetting",
        DefaultValue = "Hi {{ CurrentPerson.NickName }}! You are not currently close enough to check-in. Please try again once you're closer to the campus.",
        IsRequired = true,
        Description = "" )]

    #endregion Block Attributes for Text options
    public partial class MobileLauncher : CheckInBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DeviceIdList = "DeviceIdList";

            public const string CheckinTheme = "CheckinTheme";

            /// <summary>
            /// The checkin configuration unique identifier (which is a GroupType)
            /// </summary>
            public const string CheckinConfiguration_GroupTypeId = "CheckinConfiguration_GroupTypeId";

            /// <summary>
            /// The configured Checkin Areas (which are really Group Types)
            /// </summary>
            public const string ConfiguredAreas_GroupTypeIds = "ConfiguredAreas_GroupTypeIds";

            public const string PhoneIdentificationPage = "PhoneIdentificationPage";

            public const string LoginPage = "LoginPage";

            public const string IdentifyYouPromptTemplate = "IdentifyYouPromptTemplate";

            public const string WelcomeBackTemplate = "WelcomeBackTemplate";

            public const string AllowLocationPermissionPromptTemplate = "AllowLocationPermissionPromptTemplate";

            public const string LocationProgress = "LocationProgress";

            public const string NoScheduledDevicesAvailableTemplate = "NoScheduledDevicesAvailableTemplate";

            public const string UnableToDetermineMobileLocationTemplate = "UnableToDetermineMobileLocationTemplate";

            public const string NoDevicesFoundTemplate = "NoDevicesFoundTemplate";
        }

        #endregion Attribute Keys

        #region MessageConstants

        #endregion MessageConstants

        #region Base Control Methods

        /// <summary>
        /// Adds icons to the configuration area of a <see cref="T:Rock.Model.Block" /> instance.  Can be overridden to
        /// add additional icons
        /// </summary>
        /// <param name="canConfig">A <see cref="T:System.Boolean" /> flag that indicates if the user can configure the <see cref="T:Rock.Model.Block" /> instance.
        /// This value will be <c>true</c> if the user is allowed to configure the <see cref="T:Rock.Model.Block" /> instance; otherwise <c>false</c>.</param>
        /// <param name="canEdit">A <see cref="T:System.Boolean" /> flag that indicates if the user can edit the <see cref="T:Rock.Model.Block" /> instance.
        /// This value will be <c>true</c> if the user is allowed to edit the <see cref="T:Rock.Model.Block" /> instance; otherwise <c>false</c>.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.List`1" /> containing all the icon <see cref="T:System.Web.UI.Control">controls</see>
        /// that will be available to the user in the configuration area of the block instance.
        /// </returns>
        public override List<Control> GetAdministrateControls( bool canConfig, bool canEdit )
        {
            List<Control> configControls = new List<Control>();

            if ( canEdit )
            {
                LinkButton lbEdit = new LinkButton();
                lbEdit.CssClass = "edit";
                lbEdit.ToolTip = "Settings";
                lbEdit.Click += lbEdit_Click;
                configControls.Add( lbEdit );
                HtmlGenericControl iEdit = new HtmlGenericControl( "i" );
                lbEdit.Controls.Add( iEdit );
                lbEdit.CausesValidation = false;
                iEdit.Attributes.Add( "class", "fa fa-edit" );

                // will toggle the block config so they are no longer showing
                lbEdit.Attributes["onclick"] = "Rock.admin.pageAdmin.showBlockConfig()";

                ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( lbEdit );
            }

            configControls.AddRange( base.GetAdministrateControls( canConfig, canEdit ) );

            return configControls;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RockPage.AddScriptLink( "~/Blocks/CheckIn/Scripts/geo-min.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            if ( this.IsPostBack )
            {
                ProcessCustomPostBackEvents();
            }
            else
            {
                ShowDetails();
            }
        }

        /// <summary>
        /// Processes the custom PostBack events.
        /// </summary>
        private void ProcessCustomPostBackEvents()
        {
            /* Process GeoLocation callbacks */
            hfGetGeoLocation.Value = false.ToJavaScriptValue();
            var eventTarget = this.Request.Params["__EVENTTARGET"];
            var eventArgument = this.Request.Params["__EVENTARGUMENT"];
            if ( eventArgument.IsNotNullOrWhiteSpace() )
            {
                var parts = eventArgument.Split( '|' );

                if ( parts.Length >= 2 )
                {
                    if ( parts[0] == "GeoLocationCallback" )
                    {
                        string geoResult = parts[1];
                        string message = string.Empty;
                        if ( parts.Length == 3 )
                        {
                            message = parts[2];
                        }

                        ProcessGeolocationCallback( geoResult, message );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            bbtnPhoneLookup.Visible = false;
            bbtnLogin.Visible = false;
            bbtnGetGeoLocation.Visible = false;
            bbtnCheckin.Visible = false;
            hfGetGeoLocation.Value = false.ToJavaScriptValue();

            if ( this.InPagesConfigurationMode )
            {
                // the current page is different than the Block's page, so  we are probably on the Home > CMS Configuration > Pages page
                return;
            }

            var selectedCheckinType = GroupTypeCache.Get( this.GetAttributeValue( AttributeKey.CheckinConfiguration_GroupTypeId ).AsInteger() );
            if ( selectedCheckinType == null )
            {
                lMessage.Text = "Unable to determine check-in configuration";
                return;
            }


            var configuredTheme = this.GetAttributeValue( AttributeKey.CheckinTheme );
            SetSelectedTheme( configuredTheme );

            // Identification (Login or COOKIE_UNSECURED_PERSON_IDENTIFIER)
            Person mobilePerson = GetMobilePerson();

            if ( mobilePerson == null )
            {
                // unable to determine person from login or person cookie
                lMessage.Text = "Before we proceed we'll need you to identify you for check-in.";
                bbtnPhoneLookup.Visible = true;
                bbtnLogin.Visible = true;
                return;
            }


            bool alreadyHasGeolocation = Request.Cookies[CheckInCookieKey.RockHasLocationApproval] != null;
            if ( !alreadyHasGeolocation )
            {
                // the RockHasLocationApproval cookie indicates that location access hasn't been allowed yet
                lMessage.Text = "We need to determine your location to complete the check-in process. You'll notice a request window pop-up. Be sure to allow permissions. We'll only have permission to you location when you're visiting this site.";
                bbtnGetGeoLocation.Visible = true;
                return;
            }


            // they already approved location permissions so let the Geo Location JavaScript return the current lat/long,
            // then ProcessGeolocationCallback will take care of the rest of the logic
            // this might take a few seconds, so display a progress message
            lMessage.Text = GetMessageText( AttributeKey.LocationProgress );
            EnableGeoLocationScript();
        }

        /// <summary>
        /// Sets the configured theme and updates the theme cookie if needed
        /// </summary>
        /// <param name="theme">The theme.</param>
        private void SetSelectedTheme( string theme )
        {
            if ( LocalDeviceConfig.CurrentTheme != theme )
            {
                LocalDeviceConfig.CurrentTheme = ddlTheme.SelectedValue;
                SaveState();
            }

            if ( !RockPage.Site.Theme.Equals( LocalDeviceConfig.CurrentTheme, StringComparison.OrdinalIgnoreCase ) )
            {
                // if the site's theme doesn't match the configured theme, reload the page with the theme parameter so that the correct theme gets loaded and the theme cookie gets set
                Dictionary<string, string> themeParameters = new Dictionary<string, string>();
                themeParameters.Add( "theme", LocalDeviceConfig.CurrentTheme );

                NavigateToCurrentPageReference( themeParameters );
            }
        }

        /// <summary>
        /// Gets the mobile person.
        /// </summary>
        /// <returns></returns>
        private Person GetMobilePerson()
        {
            var rockContext = new RockContext();
            var mobilePerson = this.CurrentPerson;
            if ( mobilePerson == null )
            {
                var personAliasGuid = GetPersonAliasGuidFromUnsecuredPersonIdentifier();
                if ( personAliasGuid.HasValue )
                {
                    mobilePerson = new PersonAliasService( rockContext ).GetPerson( personAliasGuid.Value );
                }
            }

            return mobilePerson;
        }

        /// <summary>
        /// Get the unsecured person identifier from the cookie.
        /// </summary>
        private Guid? GetPersonAliasGuidFromUnsecuredPersonIdentifier()
        {
            if ( Request.Cookies[Rock.Security.Authorization.COOKIE_UNSECURED_PERSON_IDENTIFIER] != null )
            {
                return Request.Cookies[Rock.Security.Authorization.COOKIE_UNSECURED_PERSON_IDENTIFIER].Value.AsGuidOrNull();
            }

            return null;
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lbEdit_Click( object sender, EventArgs e )
        {
            ShowSettings();
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        private void ShowSettings()
        {
            BindThemes();

            ddlTheme.SetValue( this.GetAttributeValue( AttributeKey.CheckinTheme ).ToLower() );

            BindDevices();

            var selectedDevicesIds = this.GetAttributeValue( AttributeKey.DeviceIdList ).SplitDelimitedValues().AsIntegerList();

            lbDevices.SetValues( selectedDevicesIds );

            BindCheckinTypes();

            var selectedCheckinType = GroupTypeCache.Get( this.GetAttributeValue( AttributeKey.CheckinConfiguration_GroupTypeId ).AsInteger() );

            ddlCheckinType.SetValue( selectedCheckinType );

            var configuredAreas_GroupTypeIds = this.GetAttributeValue( AttributeKey.ConfiguredAreas_GroupTypeIds ).SplitDelimitedValues().AsIntegerList();

            // Bind Areas (which are Group Types)
            BindAreas( selectedDevicesIds );

            lbAreas.SetValues( configuredAreas_GroupTypeIds );

            pnlEditSettings.Visible = true;
            mdEditSettings.Show();
        }

        /// <summary>
        /// Binds the group types (checkin areas)
        /// </summary>
        /// <param name="selectedDeviceIds">The selected device ids.</param>
        private void BindAreas( IEnumerable<int> selectedDeviceIds )
        {
            // keep any currently selected areas after we repopulate areas for the selectedCheckinType
            var selectedAreaIds = lbAreas.SelectedValues.AsIntegerList();

            var rockContext = new RockContext();
            var locationService = new LocationService( rockContext );
            var groupLocationService = new GroupLocationService( rockContext );

            // Get all locations (and their children) associated with the select devices
            List<int> locationIds;
            if ( selectedDeviceIds.Any() )
            {
                locationIds = locationService
                   .GetByDevice( selectedDeviceIds, true )
                   .Select( l => l.Id )
                   .ToList();
            }
            else
            {
                locationIds = locationService
                   .GetAllDeviceLocations( true )
                   .Select( l => l.Id )
                   .ToList();
            }

            var locationGroupTypes = groupLocationService
                .Queryable().AsNoTracking()
                .Where( l => locationIds.Contains( l.LocationId ) )
                .Where( gl => gl.Group.GroupType.TakesAttendance )
                .Select( gl => gl.Group.GroupTypeId )
                .Distinct()
                .ToList()
                .Select( a => GroupTypeCache.Get( a ) )
                .OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

            lbAreas.Items.Clear();
            lbAreas.Items.AddRange( locationGroupTypes.Select( a => new ListItem( a.Name, a.Id.ToString() ) ).ToArray() );

            // restore any areas that we selected prior to repopulating the available areas
            lbAreas.SetValues( selectedAreaIds );
        }

        /// <summary>
        /// Binds the checkin types.
        /// </summary>
        private void BindCheckinTypes()
        {
            using ( var rockContext = new RockContext() )
            {
                var groupTypeService = new GroupTypeService( rockContext );

                var checkinTemplateTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );

                var checkinTypes = groupTypeService
                    .Queryable().AsNoTracking()
                    .Where( t => t.GroupTypePurposeValueId.HasValue && t.GroupTypePurposeValueId == checkinTemplateTypeId )
                    .OrderBy( t => t.Name )
                    .Select( t => new
                    {
                        t.Name,
                        t.Id
                    } )
                    .ToList();

                ddlCheckinType.Items.Clear();
                ddlCheckinType.Items.Add( new ListItem() );
                ddlCheckinType.Items.AddRange( checkinTypes.Select( a => new ListItem( a.Name, a.Id.ToString() ) ).ToArray() );
            }
        }

        /// <summary>
        /// Binds the themes.
        /// </summary>
        private void BindThemes()
        {
            ddlTheme.Items.Clear();
            ddlTheme.Items.Add( new ListItem() );
            DirectoryInfo di = new DirectoryInfo( this.Page.Request.MapPath( ResolveRockUrl( "~~" ) ) );
            foreach ( var themeDir in di.Parent.EnumerateDirectories().OrderBy( a => a.Name ) )
            {
                ddlTheme.Items.Add( new ListItem( themeDir.Name, themeDir.Name.ToLower() ) );
            }
        }

        /// <summary>
        /// Binds the devices.
        /// </summary>
        private void BindDevices()
        {
            int? kioskDeviceTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid() );

            var rockContext = new RockContext();

            DeviceService deviceService = new DeviceService( rockContext );
            var devices = deviceService.Queryable().AsNoTracking().Where( d => d.DeviceTypeValueId == kioskDeviceTypeValueId )
                .OrderBy( a => a.Name )
                .Select( a => new
                {
                    a.Id,
                    a.Name
                } ).ToList();

            lbDevices.Items.Clear();
            lbDevices.Items.Add( new ListItem() );
            lbDevices.Items.AddRange( devices.Select( a => new ListItem( a.Name, a.Id.ToString() ) ).ToArray() );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            //
        }

        /// <summary>
        /// Handles the Click event of the bbtnCheckin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bbtnCheckin_Click( object sender, EventArgs e )
        {
            var configuredCheckinTypeId = this.GetAttributeValue( AttributeKey.CheckinConfiguration_GroupTypeId ).AsIntegerOrNull();

            LocalDeviceConfig.CurrentCheckinTypeId = configuredCheckinTypeId;
            LocalDeviceConfig.CurrentGroupTypeIds = this.GetAttributeValue( AttributeKey.ConfiguredAreas_GroupTypeIds ).SplitDelimitedValues().AsIntegerList();

            LocalDeviceConfig.CurrentTheme = this.GetAttributeValue( AttributeKey.CheckinTheme );

            // TODO: Determine device by GEO location and block configuration
            //LocalDeviceConfig.CurrentKioskId = this.GetAttributeValue( AttributeKey.)

            var checkInState = new CheckInState( LocalDeviceConfig );

            // override the HomePage block setting to the mobile home page
            checkInState.HomePageOverride = this.PageCache.Guid;


            // turn off the idle redirect blocks since we don't a person's mobile device to do that
            checkInState.DisableIdleRedirect = true;


            checkInState.CheckIn = new CheckInStatus();
            checkInState.CheckIn.SearchType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_FAMILY_ID );

            var mobilePerson = this.GetMobilePerson();

            if ( mobilePerson == null )
            {
                // shouldn't happen
                return;
            }

            var family = mobilePerson.GetFamily();

            var familyMembers = mobilePerson.GetFamilyMembers( true );

            var firstNames = familyMembers
                                .OrderBy( m => m.GroupRole.Order )
                                .ThenBy( m => m.Person.BirthYear )
                                .ThenBy( m => m.Person.BirthMonth )
                                .ThenBy( m => m.Person.BirthDay )
                                .ThenBy( m => m.Person.Gender )
                                .Select( m => m.Person.NickName )
                                .ToList();

            var checkInFamily = new CheckInFamily();
            checkInFamily.Group = family.Clone( false );
            checkInFamily.Caption = family.ToString();
            checkInFamily.FirstNames = firstNames;
            checkInFamily.SubCaption = firstNames.AsDelimited( ", " );
            checkInState.CheckIn.Families.Add( checkInFamily );

            NavigateToNextPage();
        }

        #endregion

        #region Methods

        #endregion

        /// <summary>
        /// Handles the Click event of the bbtnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bbtnLogin_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "ReturnUrl", Request.RawUrl );

            NavigateToLinkedPage( "core_LoginPage", queryParams );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdEditSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdEditSettings_SaveClick( object sender, EventArgs e )
        {
            this.SetAttributeValue( AttributeKey.CheckinConfiguration_GroupTypeId, ddlCheckinType.SelectedValue );
            this.SetAttributeValue( AttributeKey.CheckinTheme, ddlTheme.SelectedValue );
            this.SetAttributeValue( AttributeKey.DeviceIdList, lbDevices.SelectedValues.AsDelimited( "," ) );
            this.SetAttributeValue( AttributeKey.ConfiguredAreas_GroupTypeIds, lbAreas.SelectedValues.AsDelimited( "," ) );
            this.SaveAttributeValues();
            mdEditSettings.Hide();
            pnlEditSettings.Visible = false;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCheckinType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCheckinType_SelectedIndexChanged( object sender, EventArgs e )
        {
            // TODO, needed?

            var selectedDeviceIds = lbDevices.SelectedValuesAsInt;
            BindAreas( selectedDeviceIds );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the lbDevices control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDevices_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedDeviceIds = lbDevices.SelectedValuesAsInt;
            BindAreas( selectedDeviceIds );
        }

        /// <summary>
        /// Handles the Click event of the bbtnPhoneLookup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bbtnPhoneLookup_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.PhoneIdentificationPage );
        }

        /// <summary>
        /// Handles the Click event of the bbtnGetGeoLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bbtnGetGeoLocation_Click( object sender, EventArgs e )
        {
            EnableGeoLocationScript();
        }

        /// <summary>
        /// Enables the GEO location JavaScript which will return the current lat/long then call <seealso cref="ProcessGeolocationCallback(string, string)"/>
        /// </summary>
        private void EnableGeoLocationScript()
        {
            hfGetGeoLocation.Value = true.ToJavaScriptValue();
        }

        /// <summary>
        /// Gets the device from the GeoLocation callback result;
        /// </summary>
        /// <param name="callbackResult">The callback result.</param>
        /// <param name="errorMessage">The error message.</param>
        private void ProcessGeolocationCallback( string callbackResult, string errorMessage )
        {
            hfGetGeoLocation.Value = false.ToJavaScriptValue();

            var latitude = hfLatitude.Value.AsDoubleOrNull();
            var longitude = hfLongitude.Value.AsDoubleOrNull();
            Device device = null;

            if ( callbackResult == "Success" && latitude.HasValue && longitude.HasValue )
            {
                bbtnGetGeoLocation.Visible = false;
                HttpCookie rockHasLocationApprovalCookie = new HttpCookie( CheckInCookieKey.RockHasLocationApproval, "true" );
                Response.Cookies.Set( rockHasLocationApprovalCookie );

                device = GetClosestKioskByGeoFencing( latitude.Value, longitude.Value );
            }
            else
            {
                lMessage.Text = GetMessageText( AttributeKey.UnableToDetermineMobileLocationTemplate );
                return;
            }

            if ( device == null )
            {
                lMessage.Text = GetMessageText( AttributeKey.NoDevicesFoundTemplate );
                return;
            }
            else
            {
                // device found for mobile person's location
                lMessage.Text = GetMessageText( AttributeKey.WelcomeBackTemplate );
                bbtnCheckin.Visible = true;
                return;
            }
        }

        /// <summary>
        /// Gets the message text.
        /// </summary>
        /// <param name="messageLavaTemplateAttributeKey">The message lava template attribute key.</param>
        /// <returns></returns>
        private string GetMessageText( string messageLavaTemplateAttributeKey )
        {
            var mobilePerson = this.GetMobilePerson();
            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, mobilePerson );

            string messageLavaTemplate = this.GetAttributeValue( messageLavaTemplateAttributeKey );

            return messageLavaTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Returns a kiosk based on finding a GEO location match for the given latitude and longitude.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns></returns>
        public Device GetClosestKioskByGeoFencing( double latitude, double longitude )
        {
            var checkInDeviceTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;

            using ( var rockContext = new RockContext() )
            {
                IQueryable<Device> kioskQuery = new DeviceService( rockContext ).GetDevicesByGeocode( latitude, longitude, checkInDeviceTypeId );
                List<int> allowedDeviceIds = this.GetAttributeValue( AttributeKey.DeviceIdList ).SplitDelimitedValues().AsIntegerList();
                if ( allowedDeviceIds.Any() )
                {
                    kioskQuery = kioskQuery.Where( a => allowedDeviceIds.Contains( a.Id ) );
                }

                var mobileGeoPoint = Location.GetGeoPoint( latitude, longitude );

                return kioskQuery.OrderBy( a => a.Location.GeoPoint.Distance( mobileGeoPoint ) ).FirstOrDefault();
            }
        }
    }
}