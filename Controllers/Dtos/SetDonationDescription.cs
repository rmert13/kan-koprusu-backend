namespace Controllers.Dtos;

public sealed record SetDonationDescriptionRequest(string DonationDescription, Guid? SessionId);
