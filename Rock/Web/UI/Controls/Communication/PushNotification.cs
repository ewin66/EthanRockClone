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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Communication;
using Rock.Model;
using Rock.Utility;

namespace Rock.Web.UI.Controls.Communication
{
    /// <summary>
    ///  Push Notification Medium Control
    /// </summary>
    public class PushNotification : MediumControl
    {
        #region UI Controls

        private RockControlWrapper rcwMessage;
        private MergeFieldPicker mfpMessage;
        private RockTextBox tbMessage;
        private RockTextBox tbTitle;
        private RockCheckBox cbSound;

        private ImageUploader iupPushImage;
        private RockRadioButtonList rbOpenAction;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the transport.
        /// </summary>
        /// <value>
        /// The transport.
        /// </value>
        public TransportComponent Transport { get; set; }

        /// <summary>
        /// Sets control values from a communication record.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public override void SetFromCommunication( CommunicationDetails communication )
        {
            EnsureChildControls();
            tbTitle.Text = communication.PushTitle;
            tbMessage.Text = communication.PushMessage;
            cbSound.Checked = communication.PushSound.IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Updates the a communication record from control values.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public override void UpdateCommunication( CommunicationDetails communication )
        {
            EnsureChildControls();
            communication.PushTitle = tbTitle.Text;
            communication.PushMessage = tbMessage.Text;
            communication.PushSound = cbSound.Checked ? "default" : string.Empty;
        }

        #endregion

        #region CompositeControl Methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            tbTitle = new RockTextBox
            {
                ID = string.Format( "tbTextTitle_{0}", this.ID ),
                TextMode = TextBoxMode.SingleLine,
                Required = false,
                Label = "Title",
                MaxLength = 100
            };
            Controls.Add( tbTitle );


            cbSound = new RockCheckBox
            {
                ID = string.Format( "cbSound_{0}", this.ID ),
                Label = "Should make sound?"
            };
            Controls.Add( cbSound );

            rcwMessage = new RockControlWrapper
            {
                ID = string.Format( "rcwMessage_{0}", this.ID ),
                Label = "Message",
                Help = "<span class='tip tip-lava'></span>"
            };
            Controls.Add( rcwMessage );

            mfpMessage = new MergeFieldPicker
            {
                ID = string.Format( "mfpMergeFields_{0}", this.ID )
            };
            mfpMessage.MergeFields.Clear();
            mfpMessage.MergeFields.Add( "GlobalAttribute" );
            mfpMessage.MergeFields.Add( "Rock.Model.Person" );
            mfpMessage.CssClass += " pull-right margin-b-sm";
            mfpMessage.SelectItem += mfpMergeFields_SelectItem;
            rcwMessage.Controls.Add( mfpMessage );

            tbMessage = new RockTextBox
            {
                ID = string.Format( "tbTextMessage_{0}", this.ID ),
                TextMode = TextBoxMode.MultiLine,
                Rows = 3,
                Required = true
            };
            rcwMessage.Controls.Add( tbMessage );

            iupPushImage = new ImageUploader
            {
                ID = $"{nameof( iupPushImage )}_{ID}",
                Label = "Image",
                Help = "We recommend an image size of 1038x520."

            };

            iupPushImage.ImageUploaded += fupPushImage_ImageUploaded;

            rcwMessage.Controls.Add( iupPushImage );

            rbOpenAction = new RockRadioButtonList
            {
                ID = $"{nameof( rbOpenAction )}_{ID}",
                Label = "Open Action",
                RepeatDirection = RepeatDirection.Horizontal,
                Help = "Defines the open action for the message."
            };

            rbOpenAction.Items.AddRange( new ListItem[] {
                new ListItem
                {
                    Text = "No Action",
                    Value = PushOpenAction.NoAction.ConvertToInt().ToString()
                },
                new ListItem
                {
                    Text = "Show Details",
                    Value = PushOpenAction.ShowDetails.ConvertToInt().ToString()
                }
            } );

            if ( Transport is IRockMobilePush )
            {
                rbOpenAction.Items.AddRange( new ListItem[] {
                    new ListItem
                    {
                        Text = "Link to Mobile Page",
                        Value = PushOpenAction.LinkToMobilePage.ConvertToInt().ToString()
                    },
                    new ListItem
                    {
                        Text = "Link to URL",
                        Value = PushOpenAction.LinkToUrl.ConvertToInt().ToString()
                    }
                } );

            }

            rcwMessage.Controls.Add( rbOpenAction );
        }

        private void fupPushImage_ImageUploaded( object sender, ImageUploaderEventArgs e )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public override string ValidationGroup
        {
            get
            {
                EnsureChildControls();
                return tbMessage.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                mfpMessage.ValidationGroup = value;
                tbMessage.ValidationGroup = value;
                tbTitle.ValidationGroup = value;
                cbSound.ValidationGroup = value;
            }
        }

        /// <summary>
        /// On new communication, initializes controls from sender values
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void InitializeFromSender( Person sender )
        {
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            tbTitle.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            cbSound.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            rcwMessage.RenderControl( writer );
        }

        #endregion

        #region Events

        void mfpMergeFields_SelectItem( object sender, EventArgs e )
        {
            EnsureChildControls();
            tbMessage.Text += mfpMessage.SelectedMergeField;
            mfpMessage.SetValue( string.Empty );
        }

        #endregion
    }
}
