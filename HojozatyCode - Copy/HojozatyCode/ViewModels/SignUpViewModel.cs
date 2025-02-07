using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HojozatyCode.Models;
using HojozatyCode.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


namespace HojozatyCode.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {

        [ObservableProperty]
        private bool isHiddenPassword = true;

        [ObservableProperty]
        private bool isHiddenConfirmPassword = true;

        [ObservableProperty]
        private User user = new User();

        public List<string> GenderOptions { get; } = new List<string> { "Male", "Female", "Other" };

        public string EyeIconPasswordSource => IsHiddenPassword ? "eye_off_icon.png" : "eye_on_icon.png";
    
        public string EyeIconConfirmPasswordSource => IsHiddenConfirmPassword ? "eye_off_icon.png" : "eye_on_icon.png";

        [ObservableProperty]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private string errorMessage;

        [RelayCommand]
        private async Task LogInAsync()
        {
            await Shell.Current.GoToAsync(nameof(Pages.LogInPage));
        }

        [RelayCommand]
        private async Task ReturnToThePreviousAsync()
        {
            await Shell.Current.GoToAsync(nameof(Pages.LoginSignupPage));
        }

        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            //Toggle Visibility
            IsHiddenPassword = !IsHiddenPassword;

            //Notfiy the UI that the eye password changed
            OnPropertyChanged(nameof(EyeIconPasswordSource));
        }

        [RelayCommand]
        private void ToggleConfirmPasswordVisibility()
        {
            //Toggle Visibility
            IsHiddenConfirmPassword = !IsHiddenConfirmPassword;

            //Notify that the eye confirm password changed
            OnPropertyChanged(nameof(EyeIconConfirmPasswordSource));
        }

        [RelayCommand]
		[Obsolete]
		private async Task SignUpAsync()
        {
            try
            {
                // // Validate the user input
                // var validationContext = new ValidationContext(User);
                // var validationResults = new List<ValidationResult>();
                // bool isValid = Validator.TryValidateObject(User, validationContext, validationResults, true);

                // if (!isValid)
                // {
                //     ErrorMessage = string.Join("\n", validationResults.Select(r => r.ErrorMessage));
                //     return;
                // }

                // Check if passwords match
                if (User.Password != ConfirmPassword)
                {
                    ErrorMessage = "Passwords do not match";
                    return;
                }
                
                // Check email pattern
                if (!Regex.IsMatch(User.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    ErrorMessage = "Invalid email format";
                    return;
                }

                // Check password pattern
                if (!Regex.IsMatch(User.Password, @"^.{8,}$"))
                {
                    ErrorMessage = "Password must be at least 8 characters long";
                    return;
                }

                var response = await SupabaseConfig.SupabaseClient.Auth.SignUp(User.Email, User.Password);
                if (response.User != null)
                {
                    User.Id = response.User.Id;
                    User.DateCreated = DateTime.UtcNow;
                    // Hash the password before saving
                    User.Password = BCrypt.Net.BCrypt.HashPassword(User.Password);
                    // Save additional user information to Supabase
                    await SupabaseConfig.SupabaseClient.From<User>().Insert(User);
                    await Shell.Current.GoToAsync(nameof(Pages.HomePage));
                }
                else
                {
                    // Handle error
                    await Application.Current.MainPage.DisplayAlert("Sign Up Failed", 
                        "An error occurred during sign up. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                await Application.Current.MainPage.DisplayAlert("Sign Up Failed", ex.Message, "OK");
            }
        }
    }
}