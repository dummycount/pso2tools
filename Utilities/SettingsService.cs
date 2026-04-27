using System.ComponentModel;
using Castle.DynamicProxy;
using Config.Net;

namespace Pso2Tools;

public static class SettingsServiceBuilder
{
	public static T BuildWithFixedNotifications<T>(this ConfigurationBuilder<T> builder)
		where T : class
	{
		var generator = new ProxyGenerator();
		var settings = builder.Build();

		if (settings is not INotifyPropertyChanged notifier)
		{
			throw new ArgumentException("Interface must implement INotifyPropertyChanged");
		}

		return generator.CreateInterfaceProxyWithTargetInterface(
			settings,
			new InterfaceInterceptor(notifier)
		);
	}

	public static T Build<T>(string settingsFilePath)
		where T : class
	{
		return new ConfigurationBuilder<T>()
			.UseIniFile(settingsFilePath)
			.BuildWithFixedNotifications();
	}
}

internal class InterfaceInterceptor : IInterceptor
{
	private PropertyChangedEventHandler? propertyChanged;
	private readonly INotifyPropertyChanged settings;

	public InterfaceInterceptor(INotifyPropertyChanged settings)
	{
		this.settings = settings;
		settings.PropertyChanged += Settings_PropertyChanged;
	}

	private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		propertyChanged?.Invoke(settings, e);
	}

	public void Intercept(IInvocation invocation)
	{
		// Work around Config.Net PropertyChanged having null for sender
		if (invocation.Method.Name == "add_PropertyChanged")
		{
			if (invocation.Arguments[0] is Delegate @delegate)
			{
				invocation.ReturnValue = propertyChanged = (PropertyChangedEventHandler?)
					Delegate.Combine(propertyChanged, @delegate);
				return;
			}
		}

		if (invocation.Method.Name == "remove_PropertyChanged")
		{
			if (invocation.Arguments[0] is Delegate @delegate)
			{
				invocation.ReturnValue = propertyChanged = (PropertyChangedEventHandler?)
					Delegate.Remove(propertyChanged, @delegate);
				return;
			}
		}

		// Work around Config.Net creating infinite loop when used in data binding
		// because it doesn't check if the value actually changed before sending
		// a property changed event.
		if (invocation.Method.Name.StartsWith("set_"))
		{
			var property = invocation.Method.Name[4..];

			var currentValue = invocation
				.InvocationTarget?.GetType()
				.GetProperty(property)
				?.GetValue(invocation.InvocationTarget);
			var newValue = invocation.Arguments[0];

			if (currentValue is null)
			{
				if (newValue is null)
				{
					return;
				}
			}
			else if (currentValue.Equals(newValue))
			{
				return;
			}
		}

		invocation.Proceed();
	}
}
