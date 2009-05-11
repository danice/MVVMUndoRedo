using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UndoRedoSample
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DemoWindow : Window
    {
        VMAgenda vmAgenda;
        
        public DemoWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DemoWindow_Loaded);
            
            Agenda agenda = new Agenda();  //this is our model Class

            agenda.Persons.Add(new Person("First Person", 18));
            agenda.Persons.Add(new Person("Second Person", 19));
            agenda.Persons.Add(new Person("Third Person", 20));
            vmAgenda = new VMAgenda(agenda);            
        }

        void DemoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = vmAgenda;
        }
    }
}
