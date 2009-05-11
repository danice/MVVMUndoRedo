// This code is an adaptation of the work from Miral licensed under a
// Creative Commons Attribution 3.0 License. The purpose of the original work 
// was to work with wpf collections in a multithreaded safe way. 
// This has been changed to mantain a collection of viewmodel objects from a model objects one.
// You can find the orginal code here http://lambert.geek.nz/2007/10/30/wpf-multithreaded-collections/

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
using System.Collections;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;

namespace MVVMUndoRedo
{
    
    public interface MirrorListConversor<T, R>
    {
        T CreateViewItem(R modelItem, int index);
        R GetModelItem(T viewItem, int index);        
    }

    public abstract class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(
              PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }
    }

    public delegate void SubmitUndoItemCollectionChanged(NotifyCollectionChangedEventArgs info);

    /// <summary>
    /// Mantains a model list whose modifications are reproduced in a viewmodel list
    /// </summary>
    /// <typeparam name="V">View objects type</typeparam>
    /// <typeparam name="M">Model objects type</typeparam>
    public class MirrorList<V, M> : Observable, IList<V>, IList, ICollection<V>, INotifyCollectionChanged
    {
        private IList<M> _ModelList;
        private List<V> _MirrorList;
        private Queue<NotifyCollectionChangedEventArgs> _Changes
            = new Queue<NotifyCollectionChangedEventArgs>();
        private object _ChangesLock = new object();
        private object _MirrorLock = new object();
        private SubmitUndoItemCollectionChanged _submitComandoCollectionChanged = null;

        MirrorListConversor<V, M> _MirrorItemConversor;

        #region Contructor
        public MirrorList(IList<M> baseList, MirrorListConversor<V, M> mirrorItemConversor)
        {
            if (baseList == null)
            {
                throw new ArgumentNullException("baseList");
            }
            this._MirrorItemConversor = mirrorItemConversor;
            

            _ModelList = baseList;
            ICollection collection = _ModelList as ICollection;
            INotifyCollectionChanged changeable =
                _ModelList as INotifyCollectionChanged;
            if (changeable == null)
            {
                throw new ArgumentException("List must support "
                  + "INotifyCollectionChanged", "baseList");
            }

            if (collection != null)
            {
                Monitor.Enter(collection.SyncRoot);
            }
            try
            {
                ResetList();
                changeable.CollectionChanged += new NotifyCollectionChangedEventHandler(changeable_CollectionChanged);                
            }
            finally
            {
                if (collection != null)
                {
                    Monitor.Exit(collection.SyncRoot);
                }
            }
        }
        #endregion

        void changeable_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ProcessChange(e);
        }

        public SubmitUndoItemCollectionChanged SubmitUndoItem
        {
            get { return _submitComandoCollectionChanged; }
            set { _submitComandoCollectionChanged = value; }
        }

        private void ResetList()
        {
            _MirrorList = new List<V>();
            int count = 0;
            //llenamos la lista a partir de los originales
            foreach (M res in _ModelList)
            {
                V viewItem = _MirrorItemConversor.CreateViewItem(res,count);
                count++;
                _MirrorList.Add(viewItem);
            }
        }

        private void RecordChange(NotifyCollectionChangedEventArgs changeInfo)
        {
            bool isFirstChange = false;
            lock (_ChangesLock)
            {
                isFirstChange = (_Changes.Count == 0);
                _Changes.Enqueue(changeInfo);
            }
            if (isFirstChange)
            {
                OnCollectionDirty();
            }
        }

        protected virtual void OnCollectionDirty()
        {
            // This is virtual so that derived classes can eg. redirect
            // this to a different thread...
            ProcessChanges();
        }

        protected void ProcessChanges()
        {
            bool locked = false;
            Monitor.Enter(_ChangesLock);
            try
            {
                locked = true;
                while (_Changes.Count > 0)
                {
                    NotifyCollectionChangedEventArgs info =
                        _Changes.Dequeue();
                    Monitor.Exit(_ChangesLock);
                    locked = false;

                    // ProcessChange occurs outside the ChangesLock,
                    // permitting other threads to queue things up behind us.
                    // Note that this means that if your change producer is
                    // running faster than your change consumer, this
                    // method may never exit.  But it does avoid making the
                    // producer wait for the consumer to process.
                    ProcessChange(info);

                    Monitor.Enter(_ChangesLock);
                    locked = true;
                }
            }
            finally
            {
                if (locked)
                {
                    Monitor.Exit(_ChangesLock);
                }
            }
        }

        private void ProcessChange(NotifyCollectionChangedEventArgs info)
        {
            lock (_MirrorLock)
            {
                V viewItem;
                bool changedCount = true;

                //campos para generar el nuevo info NotifyCollectionChangedEventArgs
                NotifyCollectionChangedEventArgs infoMirror = null;
                IList newItems;
                IList oldItems;

                switch (info.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (info.OldItems != null)
                        {
                            throw new ArgumentException("Old items present in "
                              + "Add?!", "info");
                        }
                        if (info.NewItems == null)
                        {
                            throw new ArgumentException("New items not present "
                              + "in Add?!", "info");
                        }                       
                        
                        newItems = new List<V>();
                        for (int itemIndex = 0; itemIndex < info.NewItems.Count;
                            ++itemIndex)
                        {
                            viewItem = _MirrorItemConversor.CreateViewItem((M)info.NewItems[itemIndex], info.NewStartingIndex + itemIndex);
                            newItems.Add(viewItem);
                            _MirrorList.Insert(info.NewStartingIndex + itemIndex, viewItem);

                            //ojo, aquí las notificaciones son de uno en uno (a diferencia de wpf), por eso ponemos lugo infoMirror=null
                            infoMirror = new NotifyCollectionChangedEventArgs(info.Action, viewItem, info.NewStartingIndex);
                            if (infoMirror != null)
                                OnCollectionChanged(infoMirror);

                        }
                        infoMirror = null;
                        
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (info.OldItems == null)
                        {
                            throw new ArgumentException("Old items not present "
                              + "in Remove?!", "info");
                        }
                        if (info.NewItems != null)
                        {
                            throw new ArgumentException("New items present in "
                              + "Remove?!", "info");
                        }
                        oldItems = new List<V>();
                        for (int itemIndex = 0; itemIndex < info.OldItems.Count;
                            ++itemIndex)
                        {
                            oldItems.Add(_MirrorList[itemIndex]);
                            _MirrorList.RemoveAt(info.OldStartingIndex);
                        }
                        infoMirror = new NotifyCollectionChangedEventArgs(info.Action, oldItems, info.OldStartingIndex);
                        break;

                    // this Actions are not implemented to mantain compatibility with Silverlight 2.0
                    //case NotifyCollectionChangedAction.Move:
                    //    if (info.NewItems == null)
                    //    {
                    //        throw new ArgumentException("New items not present "
                    //          + "in Move?!", "info");
                    //    }
                    //    if (info.NewItems.Count != 1)
                    //    {
                    //        throw new NotSupportedException("Move operations "
                    //          + "only supported for one item at a time.");
                    //    }
                    //    _MirrorList.RemoveAt(info.OldStartingIndex);
                    //    viewItem = _MirrorItemConversor.GetViewItem((R)info.NewItems[0], info.NewStartingIndex );
                    //    _MirrorList.Insert(info.NewStartingIndex, viewItem);
                    //    changedCount = false;
                    //    break;
                    case NotifyCollectionChangedAction.Replace:
                        if (info.OldItems == null)
                        {
                            throw new ArgumentException("Old items not present "
                              + "in Replace?!", "info");
                        }
                        if (info.NewItems == null)
                        {
                            throw new ArgumentException("New items not present "
                              + "in Replace?!", "info");
                        }
                        for (int itemIndex = 0; itemIndex < info.NewItems.Count;
                            ++itemIndex)
                        {
                            _MirrorList[info.NewStartingIndex + itemIndex]
                              = _MirrorItemConversor.CreateViewItem((M)info.NewItems[itemIndex], info.NewStartingIndex + itemIndex );
                        }
                        changedCount = false;
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        ICollection collection = _ModelList as ICollection;
                        if (collection != null)
                        {
                            Monitor.Enter(collection.SyncRoot);
                        }
                        try
                        {
                            lock (_ChangesLock)
                            {
                                ResetList();
                                _Changes.Clear();
                            }
                        }
                        finally
                        {
                            if (collection != null)
                            {
                                Monitor.Exit(collection.SyncRoot);
                            }
                        }
                        infoMirror = new NotifyCollectionChangedEventArgs(info.Action);
                        break;
                    default:
                        throw new ArgumentException("Unrecognised collection "
                          + "change operation.", "info");
                }

                if (infoMirror != null)  
                    OnCollectionChanged(infoMirror);
                OnPropertyChanged("Items[]");
                if (changedCount)
                {
                    OnPropertyChanged("Count");
                }
            }
        }


        #region List Implementation
        public object SyncRoot
        {
            get { return _MirrorLock; }
        }

        public int IndexOf(V item)
        {
            lock (_MirrorLock)
            {
                return _MirrorList.IndexOf(item);
            }
        }

        public V this[int index]
        {
            get
            {
                lock (_MirrorLock)
                {
                    return _MirrorList[index];
                }
            }
        }

        public bool Contains(V item)
        {
            lock (_MirrorLock)
            {
                return _MirrorList.Contains(item);
            }
        }

        public void CopyTo(V[] array)
        {
            lock (_MirrorLock)
            {
                _MirrorList.CopyTo(array);
            }
        }

        public void CopyTo(V[] array, int arrayIndex)
        {
            lock (_MirrorLock)
            {
                _MirrorList.CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get { lock (_MirrorLock) { return _MirrorList.Count; } }
        }

        public IEnumerator<V> GetEnumerator()
        {
            lock (_MirrorLock)
            {
                foreach (V item in _MirrorList)
                {
                    yield return item;
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null) 
            {
                CollectionChanged(this, e);
            }
        }

        #endregion

        private void ThrowReadOnly()
        {
            throw new NotSupportedException("Collection is read-only.");
        }

        #region IList<V> Members
        V IList<V>.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                ThrowReadOnly();
            }
        }

        void IList<V>.Insert(int index, V item)
        {
            ThrowReadOnly();
        }

        public void Insert(int index, M modelItem)
        {
            if (_submitComandoCollectionChanged == null)
                ThrowReadOnly();
            else
            {
                NotifyCollectionChangedEventArgs info =
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, modelItem, index);
                _submitComandoCollectionChanged(info);
            }
        }

        void IList<V>.RemoveAt(int index)
        {
            if (_submitComandoCollectionChanged == null)
                ThrowReadOnly();
            else 
            {
                M modelItem = _MirrorItemConversor.GetModelItem(this[index], index);
                NotifyCollectionChangedEventArgs info =
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, modelItem, index);
                _submitComandoCollectionChanged(info);
            }

        }
        #endregion

        #region ICollection<V> Members
        void ICollection<V>.Add(V item)
        {
            ThrowReadOnly();
        }

        void ICollection<V>.Clear()
        {
            ThrowReadOnly();
        }

        bool ICollection<V>.IsReadOnly
        {
            get { return (_submitComandoCollectionChanged != null); }
        }

        bool ICollection<V>.Remove(V item)
        {
            int index = IndexOf(item);
            (this as IList<V>).RemoveAt(index);
            return true;
            //ThrowReadOnly();
            //return false;   // never reaches here
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator
            System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region IList Members
        int IList.Add(object value)
        {
            ThrowReadOnly();
            return -1;      // never reaches here
        }

        void IList.Clear()
        {
            ThrowReadOnly();
        }

        bool IList.Contains(object value)
        {
            lock (_MirrorLock)
            {
                return ((IList)_MirrorList).Contains(value);
            }
        }

        int IList.IndexOf(object value)
        {
            lock (_MirrorLock)
            {
                return ((IList)_MirrorList).IndexOf(value);
            }
        }

        void IList.Insert(int index, object value)
        {
            ThrowReadOnly();
        }

        bool IList.IsFixedSize
        {
            get { return ((IList)_MirrorList).IsFixedSize; }
        }

        bool IList.IsReadOnly
        {
            get { return (_submitComandoCollectionChanged != null); }
        }

        void IList.Remove(object value)
        {
            ThrowReadOnly();
        }

        void IList.RemoveAt(int index)
        {
            ThrowReadOnly();
        }

        object IList.this[int index]
        {
            get
            {
                lock (_MirrorLock)
                {
                    return ((IList)_MirrorList)[index];
                }
            }
            set
            {
                ThrowReadOnly();
            }
        }
        #endregion

        #region ICollection Members
        void ICollection.CopyTo(Array array, int index)
        {
            lock (_MirrorLock)
            {
                ((IList)_MirrorList).CopyTo(array, index);
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return true; }
        }
        #endregion

    }
   
}