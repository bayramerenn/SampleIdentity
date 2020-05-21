using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleIdentity.UI.Helper
{
    public interface IPasswordReset
    {
        bool PasswordResetSendEmail(string email, string link);
        bool SendEmail(string email, string link);

    }
}
