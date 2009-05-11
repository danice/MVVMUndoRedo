using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using MVVMUndoRedo;

namespace UndoRedoSample
{
    // this is the model for the Persons viewmodel. It only has a list of persons
    public class Agenda
    {
        ObservableCollection<Person> _Persons;
        public Agenda() 
        {
            _Persons = new ObservableCollection<Person>();
        }

        public ObservableCollection<Person> Persons
        {
            get { return _Persons; }
        }          
    }

    public class Person : ModelObjectBase
    {
        private string _name;
        private int _age;

        public string Name 
        {
            get { return _name; }
            set
            {
                _name = value;
                SendPropertyChanged("Name");
            }
        }

        public int Age 
        {
            get { return _age; }
            set
            {
                _age = value;
                SendPropertyChanged("Age");
            }
        }

        public Person(string name, int age)
        {
            this.Name = name;
            this.Age = age;
        }

    }

}
