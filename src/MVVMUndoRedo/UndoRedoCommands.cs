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
using System.Windows.Input;

namespace MVVMUndoRedo
{
    public class UndoCommand : CommandModel
    {
        private IUIProject _project;

        public UndoCommand(IUIProject aproject) : base(ApplicationCommands.Undo)
        {
            _project = aproject;
        }

        public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _project.CanUndo;
            e.Handled = true;
        }

        public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _project.Undo();
        }
    }

    public class RedoCommand : CommandModel
    {
        private IUIProject _project;

        public RedoCommand(IUIProject aproject) : base(ApplicationCommands.Redo)
        {
            _project = aproject;
        }

        public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _project.CanRedo;
            e.Handled = true;
        }

        public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _project.Redo();
        }

    }
}
