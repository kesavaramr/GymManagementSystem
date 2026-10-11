using System;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Data.Repositories;
using GymManagementSystem.Models;

namespace GymManagementSystem.Views.Dialogs
{
    public partial class MemberDialog : Window
    {
        private readonly TrainerRepository _trainerRepo = new();
        public Member? MemberResult { get; private set; }
        private readonly Member? _editingMember;

        public MemberDialog(Member? existingMember = null)
        {
            InitializeComponent();
            _editingMember = existingMember;
            LoadTrainers();

            if (_editingMember != null)
            {
                TxtDialogTitle.Text = "Edit Member Details";
                BtnSave.Content = "Update Member";

                TxtFullName.Text = _editingMember.FullName;
                TxtContact.Text = _editingMember.Contact;
                TxtAddress.Text = _editingMember.Address;
                DpDob.SelectedDate = _editingMember.DateOfBirth;

                foreach (ComboBoxItem item in CmbGender.Items)
                {
                    if (item.Content.ToString() == _editingMember.Gender)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }

                if (_editingMember.AssignedTrainerId.HasValue)
                {
                    CmbTrainer.SelectedValue = _editingMember.AssignedTrainerId.Value;
                }
            }
        }

        private void LoadTrainers()
        {
            var trainers = _trainerRepo.GetAll();
            trainers.Insert(0, new Trainer { TrainerId = 0, FullName = "-- None / No Trainer --" });
            CmbTrainer.ItemsSource = trainers;
            CmbTrainer.SelectedIndex = 0;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var name = TxtFullName.Text.Trim();
            var contact = TxtContact.Text.Trim();
            var address = TxtAddress.Text.Trim();
            var gender = (CmbGender.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Male";
            var dob = DpDob.SelectedDate;

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError("Please enter the member's full name.");
                return;
            }

            if (string.IsNullOrWhiteSpace(contact))
            {
                ShowError("Please enter the contact phone number.");
                return;
            }

            int? trainerId = null;
            if (CmbTrainer.SelectedValue is int tId && tId > 0)
            {
                trainerId = tId;
            }

            MemberResult = new Member
            {
                MemberId = _editingMember?.MemberId ?? 0,
                FullName = name,
                Gender = gender,
                Contact = contact,
                Address = address,
                DateOfBirth = dob,
                RegistrationDate = _editingMember?.RegistrationDate ?? DateTime.Today,
                AssignedTrainerId = trainerId
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
