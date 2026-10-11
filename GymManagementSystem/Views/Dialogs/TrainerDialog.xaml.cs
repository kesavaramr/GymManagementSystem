using System;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Models;

namespace GymManagementSystem.Views.Dialogs
{
    public partial class TrainerDialog : Window
    {
        public Trainer? TrainerResult { get; private set; }
        private readonly Trainer? _editingTrainer;

        public TrainerDialog(Trainer? existingTrainer = null)
        {
            InitializeComponent();
            _editingTrainer = existingTrainer;

            if (_editingTrainer != null)
            {
                TxtDialogTitle.Text = "Edit Trainer Details";
                BtnSave.Content = "Update Trainer";

                TxtFullName.Text = _editingTrainer.FullName;
                TxtSpecialization.Text = _editingTrainer.Specialization;
                TxtContact.Text = _editingTrainer.Contact;

                foreach (ComboBoxItem item in CmbAvailability.Items)
                {
                    if (item.Content.ToString() == _editingTrainer.Availability)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var name = TxtFullName.Text.Trim();
            var spec = TxtSpecialization.Text.Trim();
            var contact = TxtContact.Text.Trim();
            var avail = (CmbAvailability.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Full Day";

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError("Please enter trainer's full name.");
                return;
            }

            if (string.IsNullOrWhiteSpace(spec))
            {
                ShowError("Please enter trainer's specialization.");
                return;
            }

            if (string.IsNullOrWhiteSpace(contact))
            {
                ShowError("Please enter trainer's contact number.");
                return;
            }

            TrainerResult = new Trainer
            {
                TrainerId = _editingTrainer?.TrainerId ?? 0,
                FullName = name,
                Specialization = spec,
                Contact = contact,
                Availability = avail
            };

            DialogResult = true;
            Close();
        }

        private void ShowError(string msg)
        {
            TxtError.Text = msg;
            TxtError.Visibility = Visibility.Visible;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
