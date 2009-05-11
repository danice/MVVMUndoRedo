using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MVVMUndoRedo;

namespace UndoRedoSample
{
    public class VMPerson : ViewModelBase
    {
        private Person _person;
        private IUIProject _project;
        
        public VMPerson(IUIProject aproject, Person amodel)
        {
            _person = amodel;
            _person.PropertyChanged += new PropertyChangedEventHandler(OnResourcePropertyChanged);
            _project = aproject;
        }

        public string Name 
        {
            get { return _person.Name; }
            set 
            { 
                SendUIEdit(_project, _person, "Name", value); 
            }
        }
        
        public int Age
        {
            get { return _person.Age; }
            set 
            { 
                SendUIEdit(_project, _person, "Age", value); 
            }
        }

        public void OnResourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SendPropertyChanged(e.PropertyName);
        }


    }
}
