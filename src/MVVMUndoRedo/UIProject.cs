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
using System.Net;
using System.IO;
//using System.Windows.Threading;
using System.Globalization;

namespace MVVMUndoRedo
{
    public interface IUIProject
    {
        void Submit(UndoItem op);
        void Undo();
        void Redo();
        bool CanUndo{ get; }
        bool CanRedo{ get; }
        bool Modified{ get; }
    }

    public class UIProject : IUIProject
    {
        private UndoManager _undoManager = new UndoManager();
        private bool _Modified = false;

        public void Submit(UndoItem op) 
        {
            if (op.DoCommand()) 
            {
                _undoManager.AddUndoItem(op);
                _Modified = true;
            }
        }

        public void Undo()
        {
            _undoManager.Undo();
        }

        public void Redo()
        {
            _undoManager.Redo();
        }

        public bool CanUndo
        {
            get { return _undoManager.CanUndo; }
        }

        public bool CanRedo
        {
            get { return _undoManager.CanRedo; }
        }

        public bool Modified
        {
            get { return _Modified; }
        }

    }
}
