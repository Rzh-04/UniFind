namespace UniversityLostAndFound.Models
{
    // Represents whether an item was lost or found by the reporter
    public enum ItemType
    {
        Lost = 1,
        Found = 2
    }

    // Tracks the current life cycle of an item post
    public enum ItemStatus
    {
        Reported = 1,   // Active lost/found listing
        Claimed = 2,    // Under active claim review
        Resolved = 3,   // Successfully matched and handed back
        Returned = 4    // Returned to owner
    }

    // Tracks the review status of an ownership claim
    public enum ClaimStatus
    {
        Pending = 1,    // Awaiting verification by finder or campus admin
        Approved = 2,   // Verified, item ready for pickup
        Rejected = 3    // Claim denied due to mismatch or insufficient proof
    }
}
