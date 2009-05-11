// Copyright (c) 2009 Daniel Calbet, http://danicalbet.wordpress.com
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
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;

namespace MVVMUndoRedo
{
    public class UndoItem
    {
        private Object _modelObject;

        public Object ModelObject
        {
            get { return _modelObject; }
            set { _modelObject = value; }
        }

        public virtual bool DoCommand()
        {
            return true;
        }

        public virtual void Undo()
        {
        }

        public virtual void Redo()
        {

        }
    }

    public class UIEdit : UndoItem
    {
        private string _propertyStr;
        private PropertyInfo _propInfo = null;
        private object _oldValue = null;
        private object _newValue;

        public override bool DoCommand()
        {
            _oldValue = PropInfo.GetValue(ModelObject, null);

            PropInfo.SetValue(ModelObject, _newValue, null);

            return true;
        }

        public override void Undo()
        {
            PropInfo.SetValue(ModelObject, _oldValue, null);
        }

        public override void Redo()
        {
            PropInfo.SetValue(ModelObject, _newValue, null);
        }

        public object NewValue
        {
            get { return _newValue; }
            set { _newValue = value; }
        }

        public string PropertyStr
        {
            get { return _propertyStr; }
            set { this._propertyStr = value; }
        }

        private PropertyInfo PropInfo
        {
            get
            {
                if (null == _propInfo)
                {
                    Type type = ModelObject.GetType();
                    this._propInfo = type.GetProperty(PropertyStr);
                }
                return _propInfo;
            }
        }

    }

    public class UICompo : UndoItem
    {
        private NotifyCollectionChangedEventArgs _InfoUndo = null;

        public NotifyCollectionChangedEventArgs Info { get; set; }
        public IList ModelList { get; set; }

        public override bool DoCommand()            
        {
            return DoCollectionChange(Info);
        }

        public bool DoCollectionChange(NotifyCollectionChangedEventArgs infoComm)
        {            
            switch (infoComm.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (infoComm.OldItems != null)
                    {
                        throw new ArgumentException("Old items present in "
                          + "Add?!", "info");
                    }
                    if (infoComm.NewItems == null)
                    {
                        throw new ArgumentException("New items not present "
                          + "in Add?!", "info");
                    }

                    ModelList.Insert(infoComm.NewStartingIndex, infoComm.NewItems[0]);
                    
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (infoComm.OldItems == null)
                    {
                        throw new ArgumentException("Old items not present "
                          + "in Remove?!", "info");
                    }
                    if (infoComm.NewItems != null)
                    {
                        throw new ArgumentException("New items present in "
                          + "Remove?!", "info");
                    }
                    for (int itemIndex = 0; itemIndex < infoComm.OldItems.Count;
                        ++itemIndex)
                    {
                        ModelList.RemoveAt(infoComm.OldStartingIndex);
                    }
                    break;
                //case NotifyCollectionChangedAction.Move:
                //    if (infoComm.NewItems == null)
                //    {
                //        throw new ArgumentException("New items not present "
                //          + "in Move?!", "info");
                //    }
                //    if (infoComm.NewItems.Count != 1)
                //    {
                //        throw new NotSupportedException("Move operations "
                //          + "only supported for one item at a time.");
                //    }
                //    ItemsResource.RemoveAt(infoComm.OldStartingIndex);
                //    ItemsResource.Insert(infoComm.NewStartingIndex, infoComm.NewItems[0]);
                //    break;
                case NotifyCollectionChangedAction.Replace:
                    if (infoComm.OldItems == null)
                    {
                        throw new ArgumentException("Old items not present "
                          + "in Replace?!", "info");
                    }
                    if (infoComm.NewItems == null)
                    {
                        throw new ArgumentException("New items not present "
                          + "in Replace?!", "info");
                    }
                    for (int itemIndex = 0; itemIndex < infoComm.NewItems.Count;
                        ++itemIndex)
                    {
                        ModelList[infoComm.NewStartingIndex + itemIndex]
                          = infoComm.NewItems[itemIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentException("Unrecognised collection "
                      + "change operation.", "info");
            }

            return true;
        }

        NotifyCollectionChangedEventArgs GetInfoUndo(NotifyCollectionChangedEventArgs infoComm)
        {            
            switch (infoComm.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, 
                        infoComm.NewItems[0], infoComm.NewStartingIndex);
                case NotifyCollectionChangedAction.Remove:

                    //return new NotifyCollectionChangedEventArgs(
                    //    NotifyCollectionChangedAction.Add, modelItem, index);

                    return new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        infoComm.OldItems[0],
                        infoComm.OldStartingIndex);                    
                //case NotifyCollectionChangedAction.Move:
                //    return new NotifyCollectionChangedEventArgs(
                //        NotifyCollectionChangedAction.Add,
                //        infoComm.OldItems,
                //        infoComm.OldStartingIndex);                    
                case NotifyCollectionChangedAction.Replace:
                    return new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        infoComm.OldItems[0],
                        infoComm.OldStartingIndex);
                case NotifyCollectionChangedAction.Reset:
                    return new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset);
                default:
                    throw new ArgumentException("Unrecognised collection "
                      + "change operation.", "info");
            }
        }

        public override void Undo()
        {
            if (_InfoUndo == null)
                _InfoUndo = GetInfoUndo(Info);
            DoCollectionChange(_InfoUndo);
        }

        public override void Redo()
        {
            DoCollectionChange(Info);
        }

    }
    
    
    
}
