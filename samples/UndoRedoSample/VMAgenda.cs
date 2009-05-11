using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using MVVMUndoRedo;

namespace UndoRedoSample
{
    public class VMAgenda : ViewModelBase, ICommandContext, MirrorListConversor<VMPerson, Person>
    {
        UIProject _project;
        Agenda _agenda;
        MirrorList<VMPerson, Person> _personsLists;

        private VMPerson _selectedPerson;

        public VMPerson SelectedPerson
        {
            get { return _selectedPerson; }
            set
            {
                _selectedPerson = value;
                SendPropertyChanged("SelectedPerson");
            }
        }

        public ICollection<VMPerson> PersonsList
        {
            get { return _personsLists; }
        }

        public MirrorList<VMPerson, Person> MirrorPersonsList
        {
            get { return _personsLists;}
        }        

        public Agenda Agenda 
        { 
            get { return _agenda; } 
        }

        #region Constructor
        public VMAgenda(Agenda model)
        {
            _project = new UIProject();
            _agenda = model;
            InsertPersonDialog = new VMInsertPerson(this);
            InsertPersonDialog.Visible = Visibility.Collapsed;

            
            _personsLists = new MirrorList<VMPerson, Person>(_agenda.Persons, this);
            _personsLists.SubmitUndoItem += 
                new SubmitUndoItemCollectionChanged(ComandoComponentChanged);
        }
        #endregion

        #region MirrorListConversor
        public VMPerson CreateViewItem(Person modelItem, int index) 
        {
            VMPerson newPerson = new VMPerson(_project, modelItem);
            return newPerson;
        }

        public Person GetModelItem(VMPerson viewItem, int index) 
        {
            return _agenda.Persons[index];
        }
        
        #endregion 
        
        public void ComandoComponentChanged(NotifyCollectionChangedEventArgs info)
        {
            UICompo insCompo = new UICompo();
            insCompo.ModelObject = Agenda;
            insCompo.Info = info;
            insCompo.ModelList = Agenda.Persons;
            _project.Submit(insCompo);
        }

        #region ViewModels for auxiliar views (dialogs, etc)
        public VMInsertPerson InsertPersonDialog { get; set; }
        #endregion

        #region Commands of the viewmodel
        public override void AddCommandModels(List<CommandModel> list)
        {
            list.Add(new NewCommand(this));
            list.Add(new DeleteCommand(this));
            list.Add(new UndoCommand(this._project));
            list.Add(new RedoCommand(this._project));
        }

        public class NewCommand : CommandModel
        {
            private VMAgenda _ViewModel;

            public NewCommand(VMAgenda aViewModel)
                : base(ApplicationCommands.New)
            {
                this._ViewModel = aViewModel;
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                
                if (_ViewModel.InsertPersonDialog != null)
                  e.CanExecute = _ViewModel.InsertPersonDialog.Visible != Visibility.Visible;
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                _ViewModel.InsertPersonDialog.Visible = Visibility.Visible;
            }

        }

        public class DeleteCommand : CommandModel
        {
            private VMAgenda _ViewModel;

            public DeleteCommand(VMAgenda viewModel)
                : base(ApplicationCommands.Delete)
            {
                this._ViewModel = viewModel;
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = true;
                e.Handled = true;
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                _ViewModel.PersonsList.Remove(_ViewModel.SelectedPerson);
            }
        }
        #endregion

    }
}
