// Copyright (c) 2009 Daniel Calbet, http://www.am2.es
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace MVVMUndoRedo
{
    public class ViewModelBase : INotifyPropertyChanged, ICommandContext
    {
        List<CommandModel> _commandList;
        IUIProject _project;

        public List<CommandModel> Commands
        {
            get
            {
                if (_commandList == null)
                {
                    _commandList = new List<CommandModel>();
                    this.AddCommandModels(_commandList);
                }
                
                return _commandList;
            }
        }

        public virtual void AddCommandModels(List<CommandModel> List)
        {
        }

        //public event  PropertyChangedEventHandler PropertyChanged; 
        public PropertyChangedEventHandler _propertyChangedEvent;

        ///<summary>
        ///PropertyChanged event for INotifyPropertyChanged implementation.
        ///</summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                _propertyChangedEvent += value;
            }
            remove
            {
                _propertyChangedEvent -= value;
            }
        }

        public void SendPropertyChanged(string propertyName)
        {
            if (_propertyChangedEvent != null)
            {
                _propertyChangedEvent(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void SendUIEdit(IUIProject project, object modelObject, string propStr, object newValue)
        {
            UIEdit op = new UIEdit();
            op.ModelObject = modelObject;
            op.PropertyStr = propStr;
            op.NewValue = newValue;
            project.Submit(op);
        }


    }
}
