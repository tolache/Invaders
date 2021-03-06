// -----------------------------------------------------------------------
// <copyright file="AboutControlView.xaml.cs" company="">
//
// The MIT License (MIT)
// 
// Copyright (c) 2014 Christoph Gattnar
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of
// the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// </copyright>
// -----------------------------------------------------------------------

namespace Invaders.View
{
	using System.Diagnostics;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Navigation;

	/// <summary>
	/// Interaction logic for AboutControlView.xaml
	/// </summary>
	public partial class AboutControlView : UserControl
	{
		public AboutControlView()
		{
			InitializeComponent();
		}

		private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
		{
			if(eventArgs.ChangedButton == MouseButton.Left)
			{
				Window parent = Parent as Window;
				parent?.DragMove();
			}
		}
	}
}
