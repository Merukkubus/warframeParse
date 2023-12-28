using System.Threading.Tasks;
using System.Windows;

namespace warframeParse
{
    public partial class MainWindow : Window
    {
        Report rep = new Report();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }
    }
}
