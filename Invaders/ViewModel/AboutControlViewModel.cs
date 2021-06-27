using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace Invaders.ViewModel
{
    public class AboutControlViewModel : INotifyPropertyChanged
	{
		#region Fields

		private ImageSource _applicationLogo;
		private string _title;
		private string _description;
		private string _version;
		private ImageSource _publisherLogo;
		private string _copyright;
		private string _additionalNotes;
		private string _hyperlinkText;
		private Uri _hyperlink;
        private string _publisher;
        private bool _isSemanticVersioning;

		#endregion

		#region Constructors

		public AboutControlViewModel()
		{
			Window = new Window();
			Window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			Window.SizeToContent = SizeToContent.WidthAndHeight;
			Window.ResizeMode = ResizeMode.NoResize;
			Window.WindowStyle = WindowStyle.None;

			Window.ShowInTaskbar = false;
			Window.Title = "About";
			Window.Deactivated += Window_Deactivated;

			Assembly assembly = Assembly.GetEntryAssembly();
			Version = assembly.GetName().Version.ToString();
			Title = assembly.GetName().Name;
			
			AssemblyCopyrightAttribute copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
			AssemblyDescriptionAttribute description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
			AssemblyCompanyAttribute company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();

			Copyright = copyright.Copyright;
			Description = description.Description;
			Publisher = company.Company;

			AdditionalNotes = "This is a test assignment app from Head First C# 3rd edition book." + Environment.NewLine +
			                  $"Survive {InvadersViewModel.TotalWaves} waves of evil invaders to save the earth!" + Environment.NewLine +
			                  "Special thanks to @annliseevnaart";
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the application logo.
		/// </summary>
		/// <value>The application logo.</value>
		public ImageSource ApplicationLogo
		{
			get
			{
				return _applicationLogo;
			}
			set
			{
				if(_applicationLogo != value)
				{
					_applicationLogo = value;
					OnPropertyChanged(nameof(ApplicationLogo));
				}
			}
		}

		/// <summary>
		/// Gets or sets the application title.
		/// </summary>
		/// <value>The application title.</value>
		public string Title
		{
			get
			{
				return _title;
			}
			set
			{
				if(_title != value)
				{
					_title = value;
					OnPropertyChanged(nameof(Title));
				}
			}
		}

		/// <summary>
		/// Gets or sets the application info.
		/// </summary>
		/// <value>The application info.</value>
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				if(_description != value)
				{
					_description = value;
					OnPropertyChanged(nameof(Description));
				}
			}
		}

        /// <summary>
        /// Gets or sets if Semantic Versioning is used.
        /// </summary>
        /// <see cref="http://semver.org/"/>
        /// <value>The bool that indicats whether Semantic Versioning is used.</value>
        public bool IsSemanticVersioning
        {
            get
            {
                return _isSemanticVersioning;
            }
            set
            {
                _isSemanticVersioning = value;
                OnPropertyChanged(nameof(Version));
            }
        }

		/// <summary>
		/// Gets or sets the application version.
		/// </summary>
		/// <value>The application version.</value>
		public string Version
		{
			get
			{
                if (IsSemanticVersioning)
                {
                    var tmp = _version.Split('.');
                    var version = string.Format("{0}.{1}.{2}", tmp[0], tmp[1], tmp[2]);
                    return version;
                }

				return _version;
			}
			set
			{
				if(_version != value)
				{
					_version = value;
					OnPropertyChanged(nameof(Version));
				}
			}
		}

		/// <summary>
		/// Gets or sets the publisher logo.
		/// </summary>
		/// <value>The publisher logo.</value>
		public ImageSource PublisherLogo
		{
			get
			{
				return _publisherLogo;
			}
			set
			{
				if(_publisherLogo != value)
				{
					_publisherLogo = value;
					OnPropertyChanged(nameof(PublisherLogo));
				}
			}
		}

		/// <summary>
		/// Gets or sets the publisher.
		/// </summary>
		/// <value>The publisher.</value>
		public string Publisher
		{
			get
			{
				return _publisher;
			}
			set
			{
				if(_publisher != value)
				{
					_publisher = value;
					OnPropertyChanged(nameof(Publisher));
				}
			}
		}

		/// <summary>
		/// Gets or sets the copyright label.
		/// </summary>
		/// <value>The copyright label.</value>
		public string Copyright
		{
			get
			{
				return _copyright;
			}
			set
			{
				if(_copyright != value)
				{
					_copyright = value;
					OnPropertyChanged(nameof(Copyright));
				}
			}
		}

		/// <summary>
		/// Gets or sets the hyperlink text.
		/// </summary>
		/// <value>The hyperlink text.</value>
		public string HyperlinkText
		{
			get
			{
				return _hyperlinkText;
			}
			set
			{
				try
				{
					Hyperlink = new Uri(value);
					_hyperlinkText = value;
					OnPropertyChanged(nameof(HyperlinkText));
				}
				catch
				{
				}
			}
		}

		public Uri Hyperlink
		{
			get
			{
				return _hyperlink;
			}
			set
			{
				if(_hyperlink != value)
				{
					_hyperlink = value;
					OnPropertyChanged(nameof(Hyperlink));
				}
			}
		}

		/// <summary>
		/// Gets or sets the further info.
		/// </summary>
		/// <value>The further info.</value>
		public string AdditionalNotes
		{
			get
			{
				return _additionalNotes;
			}
			set
			{
				if(_additionalNotes != value)
				{
					_additionalNotes = value;
					OnPropertyChanged(nameof(AdditionalNotes));
				}
			}
		}

		public Window Window
		{
			get;
			set;
		}

		#endregion

		void Window_Deactivated(object sender, System.EventArgs e)
		{
			Window.Close();
		}

		/// <summary>
		/// Called when a property value has changed.
		/// </summary>
		/// <param name="propertyName">The name of the property that has changed.</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if(handler != null)
			{
				PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
