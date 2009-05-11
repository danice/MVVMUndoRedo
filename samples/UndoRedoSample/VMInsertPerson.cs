using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using MVVMUndoRedo;


namespace UndoRedoSample
{
    public class VMInsertPerson : DlgViewModelBase
    {
        VMAgenda _vmPersons;
        private string _name;
        private int _age;

        public string Name {
            get { return _name; }
            set
            {
                _name = value;
                SendPropertyChanged("Name");
            }
        }

        public int Age { 
            get { return _age; } 
            set { 
                _age = value;
                SendPropertyChanged("Age");
            }
        }

        public VMInsertPerson(VMAgenda vmpersons) : base()
        {
            this._vmPersons = vmpersons;
        }

        public override void AddCommandModels(List<CommandModel> list) 
        {
            base.AddCommandModels(list);
            list.Add(new InsertPersonCommand(this));

        }

        private class InsertPersonCommand : CommandModel
        {
            private VMInsertPerson _insertPersonView;

            private VMAgenda VMPersons {
                get { return _insertPersonView._vmPersons; }
            }


            public InsertPersonCommand(VMInsertPerson insertPersonView)
                : base(ApplicationCommands.New)
            {
                this._insertPersonView = insertPersonView;
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = (_insertPersonView.Name != null);
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {                
                VMPersons.MirrorPersonsList.Insert(0, new Person(_insertPersonView.Name, _insertPersonView.Age));                
                _insertPersonView.Name = null;
                _insertPersonView.Age = 0;
                _insertPersonView.Visible = Visibility.Collapsed;
            }


        }

    }
}
