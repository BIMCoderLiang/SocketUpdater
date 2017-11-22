using System.IO;
using System.Windows;
using System.Windows.Controls;
using TestTool.View;
using TestTool.ViewModel;

namespace TestTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ToolWindow : Window
    {
        public RegistryInfoPage RegistryPage = null;
        public ClientRequestInfoPage RequestInfoPage = null;
        public ClientDownLoadPage DownLoadPage = null;
        public RestartPage RestartPage = null;

        public ToolWindow()
        {
            InitializeComponent();
            var mainFolder = Path.Combine(Path.GetTempPath(), "BIMProductUpdate");
            if (Directory.Exists(mainFolder))
            {
                Directory.Delete(mainFolder, true);
            }

            RegistryPage = new RegistryInfoPage();
            RegistryPage.DataContext = new RegistryInfoViewModel();
            StepGrid.Children.Add(RegistryPage);
        }

        private void CloseBtn_OnClick(object sender, RoutedEventArgs e)
        {            
            Close();
        }

        private void Steps_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StepGrid == null)
            {
                return;
            }
            if (Steplbx.SelectedIndex == 0)
            {
                if (null == RegistryPage)
                {
                    RegistryPage = new RegistryInfoPage();
                    RegistryPage.DataContext = new RegistryInfoViewModel();
                    StepGrid.Children.Add(RegistryPage);
                }
                HideOtherControl(RegistryPage);
            }
            if (Steplbx.SelectedIndex == 1)
            {
                if (null == RequestInfoPage)
                {
                    RequestInfoPage = new ClientRequestInfoPage();                        
                    RequestInfoPage.DataContext = new ClientRequestInfoViewModel();
                    StepGrid.Children.Add(RequestInfoPage);
                }              
                HideOtherControl(RequestInfoPage);
            }
            if (Steplbx.SelectedIndex == 2)
            {
                if (null == DownLoadPage)
                {
                    DownLoadPage = new ClientDownLoadPage();
                    var vm = (ClientRequestInfoViewModel) RequestInfoPage.DataContext;
                    DownLoadPage.DataContext = new ClientDownLoadViewModel(vm.ClientInfo, vm.DownloadInfo);
                    StepGrid.Children.Add(DownLoadPage);
                }
                HideOtherControl(DownLoadPage);
            }
            if (Steplbx.SelectedIndex == 3)
            {
                if (null == RestartPage)
                {
                    RestartPage = new RestartPage();
                    var vm = (ClientRequestInfoViewModel)RequestInfoPage.DataContext;
                    RestartPage.DataContext = new RestartViewModel(vm.DownloadInfo.LatestProductVersion);
                    StepGrid.Children.Add(RestartPage);
                }
                HideOtherControl(RestartPage);
            }
        }

        private void HideOtherControl(UserControl userCtl)
        {
            foreach (UIElement child in StepGrid.Children)
            {
                child.Visibility = Visibility.Hidden;
            }
            userCtl.Visibility = Visibility.Visible;
        }
    }
}
