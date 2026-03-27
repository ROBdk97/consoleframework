using ConsoleFramework.Events;

namespace ConsoleFramework.Controls
{
    public class RadioGroup : Panel
    {
        private int? selectedItemIndex;
        public int? SelectedItemIndex
        {
            get => selectedItemIndex;
            set
            {
                if (selectedItemIndex == value) return;
                selectedItemIndex = value;
                RaisePropertyChanged(nameof(SelectedItemIndex));
                RaisePropertyChanged(nameof(SelectedItem));
            }
        }

        public RadioButton SelectedItem =>
            selectedItemIndex.HasValue ? (RadioButton)((Control)this).Children[selectedItemIndex.Value] : null;

        public RadioGroup()
        {
            Children.ControlAdded += OnControlAdded;
            Children.ControlRemoved -= OnControlRemoved;
        }

        private void OnControlRemoved(Control control)
        {
            if (control is not RadioButton radioButton) return;
            radioButton.OnClick -= RadioButton_OnClick;
        }

        private void OnControlAdded(Control control)
        {
            if (control is not RadioButton radioButton) return;
            radioButton.OnClick += RadioButton_OnClick;
            int index = ((Control)this).Children.IndexOf(radioButton);
            radioButton.Checked = selectedItemIndex is not null && selectedItemIndex == index;
        }

        private void RadioButton_OnClick(object sender, RoutedEventArgs args)
        {
            foreach (var child in Children)
            {
                if (child is RadioButton rb && child != sender)
                    rb.Checked = false;
            }
            ((RadioButton)sender).Checked = true;
            SelectedItemIndex = ((Control)this).Children.IndexOf((Control)sender);
        }
    }
}
