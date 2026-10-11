using System;
using System.Windows;
using System.Windows.Controls;
using GymManagementSystem.Data.Repositories;
using GymManagementSystem.Models;
using GymManagementSystem.Views.Dialogs;

namespace GymManagementSystem.Views
{
    public partial class TrainersView : UserControl
    {
        private readonly TrainerRepository _trainerRepo = new();

        public TrainersView()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadTrainers();
        }

        public void LoadTrainers()
        {
            if (DgTrainers == null) return;
            var search = TxtSearch?.Text?.Trim();
            var list = string.IsNullOrEmpty(search) ? _trainerRepo.GetAll() : _trainerRepo.Search(search);
            DgTrainers.ItemsSource = list;
            if (TxtFooterCount != null)
            {
                TxtFooterCount.Text = $"Showing {list.Count} trainer(s)";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadTrainers();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Text = string.Empty;
            LoadTrainers();
        }

        private void BtnAddTrainer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TrainerDialog
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() == true && dialog.TrainerResult != null)
            {
                int newId = _trainerRepo.Add(dialog.TrainerResult);
                LoadTrainers();
                MessageBox.Show($"Trainer '{dialog.TrainerResult.FullName}' added successfully! (ID: TRN-{newId:D3})", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnEditTrainer_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Trainer trainer)
            {
                var dialog = new TrainerDialog(trainer)
                {
                    Owner = Window.GetWindow(this)
                };
                if (dialog.ShowDialog() == true && dialog.TrainerResult != null)
                {
                    _trainerRepo.Update(dialog.TrainerResult);
                    LoadTrainers();
                    MessageBox.Show($"Trainer '{dialog.TrainerResult.FullName}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnDeleteTrainer_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Trainer trainer)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete trainer '{trainer.FullName}'?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _trainerRepo.Delete(trainer.TrainerId);
                    LoadTrainers();
                    MessageBox.Show("Trainer deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
