namespace GS.MFH.PMC.Portal.Sitefinity.Infrastructure.GoogleRecaptcha
{
    internal class GoogleRecaptchaResponse
    {
        public static readonly GoogleRecaptchaResponse Valid = new GoogleRecaptchaResponse(true, string.Empty);
        public static readonly GoogleRecaptchaResponse CaptchaRequired = new GoogleRecaptchaResponse(false, "captcha-required");
        public static readonly GoogleRecaptchaResponse InvalidCaptcha = new GoogleRecaptchaResponse(false, "incorrect-captcha");
        public static readonly GoogleRecaptchaResponse GoogleRecaptchaNotReachable = new GoogleRecaptchaResponse(false, "recaptcha-not-reachable");

        internal GoogleRecaptchaResponse(bool isValid, string errorCode)
        {
            IsValid = isValid;
            ErrorCode = errorCode;
        }

        public bool IsValid { get; }

        public string ErrorCode { get; }

        public override bool Equals(object obj)
        {
            var googleRecaptchaResponse = (GoogleRecaptchaResponse)obj;
            if (googleRecaptchaResponse == null || googleRecaptchaResponse.IsValid != IsValid)
                return false;
            return googleRecaptchaResponse.ErrorCode == ErrorCode;
        }

        public override int GetHashCode()
        {
            return IsValid.GetHashCode() ^ ErrorCode.GetHashCode();
        }
    }
}