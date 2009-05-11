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
using System.Text;
using System.Windows;
using System.Windows.Input;


namespace MVVMUndoRedo
{

    public interface ICommandContext
    {
        List<CommandModel> Commands { get; }
    }
    
    public static class CreateCommandContext
    {
        public static readonly DependencyProperty ContextProperty
           = DependencyProperty.RegisterAttached("Context", typeof(ICommandContext), typeof(CreateCommandContext),
                new PropertyMetadata(new PropertyChangedCallback(OnCommandInvalidated)));

        public static ICommandContext GetContext(UIElement element)
        {
            return (ICommandContext)element.GetValue(ContextProperty);
        }

        public static void SetContext(UIElement element, ICommandContext commandContext)
        {
            element.SetValue(ContextProperty, commandContext);
        }

        /// <summary>
        /// Callback when the Command property is set or changed.
        /// </summary>
        private static void OnCommandInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            // Clear the exisiting bindings on the element we are attached to.
            UIElement element = (UIElement)dependencyObject;
            element.CommandBindings.Clear();

            // If we're given a command model, set up a binding
            ICommandContext commandContext = e.NewValue as ICommandContext;
            if (commandContext != null)
            {
                foreach (CommandModel commandModel in commandContext.Commands)
                {
                    element.CommandBindings.Add(new CommandBinding(commandModel.Command, commandModel.OnExecute, commandModel.OnQueryEnabled));
                }
                
            }

            // Suggest to WPF to refresh commands
            CommandManager.InvalidateRequerySuggested();
        }

    }
    
}
