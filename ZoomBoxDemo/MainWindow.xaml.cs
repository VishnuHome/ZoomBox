using System.Windows;

namespace ZoomBoxDemo
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		//public TestVM TestContext { get; set; }

		public MainWindow()
		{
			//this.TestContext = new TestVM();
			InitializeComponent();
			this.Loaded += MainWindow_Loaded;
		}

		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			//this.TestContext.LoadData();
		}
	}
}
