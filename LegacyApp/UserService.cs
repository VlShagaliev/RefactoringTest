using System;

namespace LegacyApp
{
    public class UserService
    {
        private void CheckCredit(User user)
        {
            switch (user.Client.Name)
            {
                case "VeryImportantClient": // Пропустить проверку лимита
                    user.HasCreditLimit = false;
                    break;

                case "ImportantClient": // Проверить лимит и удвоить его
                    user.HasCreditLimit = true;
                    using (var userCreditService = new UserCreditServiceClient())
                    {
                        var creditLimit = userCreditService.GetCreditLimit(user.FirstName, user.Surname, user.DateOfBirth);
                        creditLimit *= 2;
                        user.CreditLimit = creditLimit;
                    }
                    break;

                default: // Проверить лимит
                    user.HasCreditLimit = true;
                    using (var userCreditService = new UserCreditServiceClient())
                    {
                        var creditLimit = userCreditService.GetCreditLimit(user.FirstName, user.Surname, user.DateOfBirth);
                        user.CreditLimit = creditLimit;
                    }
                    break;
            }
        }

        public bool AddUser(string firstName, string surname, string email, DateTime dateOfBirth, int clientId)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(surname)) return false;

            if (!email.Contains("@") && !email.Contains(".")) return false;

            var timeNow = DateTime.Now;
            int userAge = timeNow.Year - dateOfBirth.Year;

            if (timeNow.Month < dateOfBirth.Month || (timeNow.Month == dateOfBirth.Month && timeNow.Day < dateOfBirth.Day)) userAge--;

            if (userAge < 21) return false;

            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                Surname = surname
            };

            CheckCredit(user);

            if (user.HasCreditLimit && user.CreditLimit < 500) return false;

            UserDataAccess.AddUser(user);

            return true;
        }
    }
}