using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace SkiaSharpDemo.Views
{
	public class TapLabel : Label
	{
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(
			nameof(Command),
			typeof(ICommand),
			typeof(TapLabel),
			null);

		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
			nameof(CommandParameter),
			typeof(object),
			typeof(TapLabel),
			null);

		public TapLabel()
		{
			var tap = new TapGestureRecognizer();
			tap.Tapped += OnTapped;

			GestureRecognizers.Add(tap);
		}

		public ICommand? Command
		{
			get => (ICommand?)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public object? CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		public event EventHandler? Tapped;

		protected virtual void OnTapped(object sender, EventArgs e)
		{
			var param = CommandParameter;
			if (Command is ICommand cmd && cmd.CanExecute(param))
				cmd.Execute(param);

			Tapped?.Invoke(this, e);
		}
	}
}
