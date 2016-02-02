using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace MvvmEventBinding
{
	/// <summary>
	/// Custom markup extension that allows direct binding of Commands to events.
	/// </summary>
	public class EventBindingExtension : MarkupExtension
	{
		/// <summary>
		/// Name of the Command to be invoked when the event fires
		/// </summary>
		private readonly string _commandName;

		public EventBindingExtension(string command)
		{
			_commandName = command;
		}

		/// <summary>
		/// Retrieves the context in which the markup extension is used, and (if used in the
		/// context of an event or a method) returns an event handler that executes the
		/// desired Command.
		/// </summary>
		/// <param name="serviceProvider"></param>
		/// <returns></returns>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			// Retrieve a reference to the InvokeCommand helper method declared below, using reflection
			MethodInfo invokeCommand = GetType().GetMethod("InvokeCommand", BindingFlags.Instance | BindingFlags.NonPublic);
			if (invokeCommand != null)
			{
				// Check if the current context is an event or a method call with two parameters
				var target = serviceProvider.GetService(typeof (IProvideValueTarget)) as IProvideValueTarget;
				if (target != null)
				{
					var property = target.TargetProperty;
					if (property is EventInfo)
					{
						// If the context is an event, simply return the helper method as delegate
						// (this delegate will be invoked when the event fires)
						var eventHandlerType = (property as EventInfo).EventHandlerType;
						return invokeCommand.CreateDelegate(eventHandlerType, this);
					}
					else if (property is MethodInfo)
					{
						// Some events are represented as method calls with 2 parameters:
						// The first parameter is the control that acts as the event's sender,
						// the second parameter is the actual event handler
						var methodParameters = (property as MethodInfo).GetParameters();
						if (methodParameters.Length == 2)
						{
							var eventHandlerType = methodParameters[1].ParameterType;
							return invokeCommand.CreateDelegate(eventHandlerType, this);
						}
					}
				}
			}
			throw new InvalidOperationException("The EventBinding markup extension is valid only in the context of events.");
		}

		/// <summary>
		/// Helper method that retrieves a control's ViewModel, searches the ViewModel for a
		/// Command with given name, and invokes this Command.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void InvokeCommand(object sender, EventArgs args)
		{
			if (!String.IsNullOrEmpty(_commandName))
			{
				var control = sender as FrameworkElement;
				if (control != null)
				{
					// Find control's ViewModel
					var viewmodel = control.DataContext;
					if (viewmodel != null)
					{
						// Command must be declared as public property within ViewModel
						var commandProperty = viewmodel.GetType().GetProperty(_commandName);
						if (commandProperty != null)
						{
							var command = commandProperty.GetValue(viewmodel) as ICommand;
							if (command != null)
							{
								// Execute Command and pass event arguments as parameter
								if (command.CanExecute(args))
								{
									command.Execute(args);
								}
							}
						}
					}
				}
			}
		}
	}
}
