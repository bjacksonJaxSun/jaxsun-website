# JaxSun.us Website

A professional ASP.NET Core MVC website for JaxSun.us - a father-son partnership focused on building ideas into software businesses.

## Project Overview

This website serves as the primary platform to attract potential partners with great business ideas and showcase the value proposition of professional product research, development, and ongoing support with a revenue-sharing model.

## Features

### Phase 1 (MVP) - Completed
- ✅ Responsive homepage with hero section and value proposition
- ✅ About section with personal stories and team profiles
- ✅ Process explanation page with step-by-step partnership journey
- ✅ Partnership model details with risk mitigation information
- ✅ Idea submission form with validation and email integration
- ✅ Contact page with multiple contact methods
- ✅ Responsive design optimized for mobile and desktop
- ✅ Midwest professional theme with brand colors
- ✅ Email confirmation and notification system
- ✅ Privacy policy and data protection information

### Phase 2 (Future Enhancements)
- [ ] Blog system implementation
- [ ] Portfolio/case study galleries
- [ ] Advanced analytics integration
- [ ] SEO optimization
- [ ] Content management system
- [ ] Performance optimization

## Technology Stack

- **Framework:** ASP.NET Core 8.0 MVC
- **Styling:** Bootstrap 5 + Custom CSS
- **JavaScript:** Vanilla JS with modern features
- **Email:** SMTP integration for form submissions
- **Data Storage:** JSON file storage (SQLite/SQL Server ready)
- **Hosting:** Azure/Cloud platform ready

## Project Structure

```
JaxSun.Web/
├── Controllers/
│   ├── HomeController.cs          # Homepage and general pages
│   ├── AboutController.cs         # About/team information
│   ├── ProcessController.cs       # Partnership process
│   ├── PartnershipController.cs   # Partnership model
│   └── ContactController.cs       # Contact and idea submission
├── Models/
│   ├── IdeaSubmissionModel.cs     # Business idea submission
│   ├── ContactModel.cs            # General contact form
│   ├── ErrorViewModel.cs          # Error handling
│   └── ViewModels/
│       └── HomeViewModel.cs       # Homepage data structures
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml           # Homepage
│   │   └── Privacy.cshtml         # Privacy policy
│   ├── About/
│   │   └── Index.cshtml           # About us page
│   ├── Process/
│   │   └── Index.cshtml           # Partnership process
│   ├── Partnership/
│   │   └── Index.cshtml           # Partnership model
│   ├── Contact/
│   │   ├── Index.cshtml           # Contact page
│   │   ├── SubmitIdea.cshtml      # Idea submission form
│   │   └── ThankYou.cshtml        # Submission confirmation
│   └── Shared/
│       ├── _Layout.cshtml         # Main layout template
│       ├── Error.cshtml           # Error page
│       ├── _ViewStart.cshtml      # View configuration
│       └── _ViewImports.cshtml    # Global imports
├── Services/
│   ├── IEmailService.cs           # Email service interface
│   ├── EmailService.cs            # Email implementation
│   ├── IIdeaSubmissionService.cs  # Submission service interface
│   └── IdeaSubmissionService.cs   # Submission implementation
├── wwwroot/
│   ├── css/
│   │   └── site.css               # Custom Midwest theme
│   ├── js/
│   │   └── site.js                # Custom JavaScript
│   ├── images/
│   │   └── placeholder.txt        # Image requirements
│   └── favicon.ico                # Site icon
├── Program.cs                     # Application startup
├── appsettings.json              # Configuration
└── JaxSun.Web.csproj            # Project file
```

## Setup Instructions

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code
- SMTP email service (Gmail, SendGrid, etc.)

### Installation

1. **Clone or extract the project:**
   ```bash
   cd /path/to/JaxSun.us
   ```

2. **Restore NuGet packages:**
   ```bash
   cd JaxSun.Web
   dotnet restore
   ```

3. **Configure email settings:**
   Edit `appsettings.json` and update the email configuration:
   ```json
   {
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "FromEmail": "contact@jaxsun.us",
       "FromName": "JaxSun Partnership",
       "Username": "your-email@gmail.com",
       "Password": "your-app-password"
     }
   }
   ```

4. **Add required images:**
   Place the following images in `wwwroot/images/`:
   - `midwest-partnership.jpg` (1200x800px) - Hero image
   - `bobby-profile.jpg` (400x400px) - Bobby's profile photo
   - `robert-profile.jpg` (400x400px) - Robert's profile photo
   - `partnership-success.jpg` (800x600px) - Partnership success image
   - `revenue-sharing-model.jpg` (600x400px) - Revenue model diagram
   
   See `wwwroot/images/placeholder.txt` for detailed image requirements.

5. **Run the application:**
   ```bash
   dotnet run
   ```

6. **Access the website:**
   Open your browser and navigate to `https://localhost:5001`

## Configuration

### Email Settings
Configure SMTP settings in `appsettings.json`:
- **SmtpServer:** Your SMTP server (e.g., smtp.gmail.com)
- **SmtpPort:** SMTP port (usually 587 for TLS)
- **FromEmail:** The email address that sends notifications
- **FromName:** Display name for outgoing emails
- **Username:** SMTP authentication username
- **Password:** SMTP authentication password (use app passwords for Gmail)

### Environment Variables
For production, set sensitive configuration as environment variables:
- `EmailSettings__Username`
- `EmailSettings__Password`

## Key Features Explained

### Idea Submission Process
1. User fills out comprehensive idea submission form
2. Form validation ensures all required fields are completed
3. Submission is saved to JSON file (easily upgradeable to database)
4. Confirmation email sent to user
5. Notification email sent to JaxSun team
6. User redirected to thank you page with next steps

### Email Integration
- **Confirmation emails:** Professional HTML emails sent to users
- **Notification emails:** Detailed submission information sent to team
- **Error handling:** Graceful fallback if email service fails
- **Template system:** Easily customizable email templates

### Responsive Design
- **Mobile-first:** Optimized for all device sizes
- **Bootstrap 5:** Modern responsive framework
- **Custom CSS:** Midwest professional theme
- **Accessibility:** WCAG 2.1 AA compliance ready

## Deployment

### Azure App Service
1. Create Azure App Service
2. Configure deployment source (GitHub, Azure DevOps, etc.)
3. Set environment variables for email configuration
4. Deploy application

### Other Hosting Providers
The application is compatible with any hosting provider that supports:
- .NET 8.0 runtime
- HTTPS
- File system access (for JSON storage)

## Maintenance

### Content Updates
- Team profiles and experience updates
- Success stories and case studies
- Blog content (future implementation)
- Seasonal imagery and messaging

### Technical Maintenance
- Regular security updates
- Performance monitoring
- Form submission monitoring
- Email delivery verification

## Support

For technical support or questions about the website:
- **Email:** contact@jaxsun.us
- **Documentation:** See individual file comments for detailed implementation notes

## License

This project is proprietary to JaxSun.us. All rights reserved.

---

**JaxSun.us** - Turning Great Ideas Into Thriving Software Businesses
*A father-son partnership bringing Midwest values and enterprise expertise to entrepreneurs*