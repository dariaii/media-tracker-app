# Media Tracker

A unified media tracker application built with ASP.NET Core MVC that allows users to subscribe to various media channels (Spotify, YouTube, News, Podcasts, etc.) and receive notifications when new content is published.

## Features

- **User Authentication & Authorization**: Built with ASP.NET Core Identity
- **Generic Repository Pattern**: Flexible data access layer for all entities
- **Service Layer Architecture**: Business logic separated from controllers
- **Background Job Processing**: Hangfire for scheduled content checking
- **SQLite Database**: Lightweight and portable database solution
- **Multiple Media Types**: Support for Spotify, YouTube, News, Podcasts, RSS, and more

## Technology Stack

- **.NET 8.0**: Latest .NET framework
- **ASP.NET Core MVC**: Web framework
- **Entity Framework Core**: ORM for database operations
- **SQLite**: Database engine
- **ASP.NET Core Identity**: Authentication and authorization
- **Hangfire**: Background job processing and scheduling

## Project Structure

```
MediaTracker/
??? Configuration/         # Application configuration classes
?   ??? HangfireAuthorizationFilter.cs
?   ??? HangfireConfiguration.cs
??? Controllers/          # MVC Controllers
?   ??? AccountController.cs
?   ??? HomeController.cs
?   ??? NotificationController.cs
?   ??? SubscriptionController.cs
??? Data/                 # Database context
?   ??? ApplicationDbContext.cs
??? Jobs/                 # Hangfire background jobs
?   ??? MediaTrackerJob.cs
??? Models/               # Domain models and view models
?   ??? ApplicationUser.cs
?   ??? ErrorViewModel.cs
?   ??? LoginViewModel.cs
?   ??? MediaContent.cs
?   ??? MediaType.cs
?   ??? Notification.cs
?   ??? RegisterViewModel.cs
?   ??? Subscription.cs
??? Repositories/         # Data access layer
?   ??? Interfaces/
?   ?   ??? IRepository.cs
?   ?   ??? IUnitOfWork.cs
?   ??? Implementations/
?       ??? Repository.cs
?       ??? UnitOfWork.cs
??? Services/            # Business logic layer
    ??? Interfaces/
    ?   ??? IMediaContentService.cs
    ?   ??? IMediaTrackerService.cs
    ?   ??? INotificationService.cs
    ?   ??? ISubscriptionService.cs
    ??? Implementations/
        ??? MediaContentService.cs
        ??? MediaTrackerService.cs
        ??? NotificationService.cs
        ??? SubscriptionService.cs
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd MediaTracker
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Create the database**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - Application: https://localhost:5001
   - Hangfire Dashboard: https://localhost:5001/hangfire

## Database Schema

### ApplicationUser
Extends IdentityUser with additional properties:
- FirstName, LastName
- CreatedAt, LastLoginAt
- Navigation properties to Subscriptions and Notifications

### Subscription
- User subscription to media channels
- Tracks channel URL, name, media type
- Maintains last check timestamp and activity status

### MediaContent
- Represents individual pieces of content from subscriptions
- Stores title, description, URL, publish date
- Tracks notification status

### Notification
- User notifications for new content
- Read/unread status tracking
- Links to users and media content

## Configuration

### Connection String
Update `appsettings.json` to modify the database location:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=mediatracker.db"
  }
}
```

### Hangfire Schedule
Modify `Configuration/HangfireConfiguration.cs` to adjust the checking frequency:
```csharp
RecurringJob.AddOrUpdate<MediaTrackerJob>(
    "check-all-subscriptions",
    job => job.CheckAllSubscriptionsJob(),
    "*/15 * * * *"); // Every 15 minutes
```

## Usage

### User Registration and Login
1. Navigate to `/Account/Register` to create a new account
2. Login at `/Account/Login`

### Managing Subscriptions
1. Go to `/Subscription/Index` to view your subscriptions
2. Click "Create" to add a new subscription
3. Provide channel URL, name, and select media type

### Viewing Notifications
1. Access `/Notification/Index` to see all notifications
2. Mark notifications as read individually or all at once

### Hangfire Dashboard
- Access `/hangfire` to view job status and history
- Monitor recurring jobs and their execution

## Extending the Application

### Adding New Media Type Support

1. **Update MediaType enum** (`Models/MediaType.cs`):
   ```csharp
   public enum MediaType
   {
       // ... existing types
       NewMediaType
   }
   ```

2. **Implement media fetching logic** (`Services/Implementations/MediaTrackerService.cs`):
   ```csharp
   private async Task<IEnumerable<MediaContent>> FetchNewContentAsync(Subscription subscription)
   {
       return subscription.MediaType switch
       {
           MediaType.NewMediaType => await FetchFromNewMediaType(subscription),
           // ... other cases
           _ => new List<MediaContent>()
       };
   }
   ```

3. **Create integration service** (optional):
   ```csharp
   public interface INewMediaTypeService
   {
       Task<IEnumerable<MediaContent>> FetchLatestContentAsync(string channelUrl);
   }
   ```

## API Integrations (To Be Implemented)

The following integrations need to be implemented in `MediaTrackerService`:

- **Spotify API**: For podcast and music updates
- **YouTube Data API**: For video uploads
- **RSS Feeds**: Generic RSS feed parser
- **News APIs**: Various news sources
- **Custom integrations**: Per your requirements

## Background Jobs

### Configured Jobs

1. **Check All Subscriptions**: Runs every 15 minutes
   - Checks all active subscriptions for new content
   - Creates notifications for users

2. **Custom Jobs**: Add your own recurring or fire-and-forget jobs
   ```csharp
   RecurringJob.AddOrUpdate<MediaTrackerJob>(
       "job-id",
       job => job.YourJobMethod(),
       Cron.Daily);
   ```

## Security Considerations

- Change default password requirements in `Program.cs`
- Implement proper authorization for Hangfire dashboard in production
- Use environment variables for sensitive configuration
- Enable HTTPS in production
- Implement rate limiting for API calls

## Future Enhancements

- [ ] Implement actual media API integrations
- [ ] Add email notifications
- [ ] Create mobile-responsive views
- [ ] Add user preferences for notification frequency
- [ ] Implement content filtering and categorization
- [ ] Add analytics dashboard
- [ ] Support for webhooks from media platforms
- [ ] Implement caching for better performance

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License.

## Support

For issues and questions, please create an issue in the repository.
