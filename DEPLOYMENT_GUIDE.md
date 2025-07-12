# JaxSun.us Render Deployment Guide

## Prerequisites
- Render account (free tier available)
- GitHub repository with your project
- Email service credentials (Gmail App Password recommended)

## Step 1: Prepare Your Repository

1. **Push to GitHub:**
   ```bash
   cd C:\Development\JaxSun.us
   git init
   git add .
   git commit -m "Initial JaxSun.us website"
   git branch -M main
   git remote add origin https://github.com/yourusername/jaxsun-website.git
   git push -u origin main
   ```

## Step 2: Deploy to Render

1. **Login to Render:**
   - Go to https://render.com
   - Login to your account

2. **Create New Web Service:**
   - Click "New +" → "Web Service"
   - Connect your GitHub repository
   - Select the JaxSun.us repository

3. **Configure Service:**
   - **Name:** `jaxsun-website`
   - **Environment:** `Docker` or `Native Environment`
   - **Build Command:** `dotnet publish JaxSun.Web/JaxSun.Web.csproj -c Release -o out`
   - **Start Command:** `dotnet out/JaxSun.Web.dll`

4. **Environment Variables:**
   Add these in the Render dashboard under "Environment":
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://0.0.0.0:10000
   EmailSettings__SmtpServer=smtp.gmail.com
   EmailSettings__SmtpPort=587
   EmailSettings__FromEmail=contact@jaxsun.us
   EmailSettings__FromName=JaxSun Partnership
   EmailSettings__Username=[Your Gmail Email]
   EmailSettings__Password=[Your Gmail App Password]
   ```

## Step 3: Configure Email (Gmail)

1. **Enable 2-Factor Authentication** on your Gmail account
2. **Generate App Password:**
   - Go to Google Account settings
   - Security → 2-Step Verification → App passwords
   - Generate password for "Mail"
3. **Use App Password** in `EmailSettings__Password` environment variable

## Step 4: Custom Domain (Optional)

1. **In Render Dashboard:**
   - Go to your service settings
   - Click "Custom Domains"
   - Add `jaxsun.us` and `www.jaxsun.us`

2. **Update DNS Records:**
   ```
   Type: CNAME
   Name: www
   Value: jaxsun-website.onrender.com

   Type: A
   Name: @
   Value: [Render's IP - provided in dashboard]
   ```

## Step 5: SSL Certificate

Render automatically provides SSL certificates for custom domains.

## Alternative: Quick Deploy Button

You can also use the render.yaml file by:
1. Pushing your code to GitHub
2. Going to https://render.com/deploy
3. Connecting your repository
4. Render will automatically use the render.yaml configuration

## Post-Deployment Checklist

✅ **Test Website:** Visit your Render URL to verify deployment
✅ **Test Contact Form:** Ensure email functionality works
✅ **Test Mobile:** Verify responsive design
✅ **Check Images:** Ensure all images load correctly
✅ **Test Navigation:** Verify all menu links work
✅ **Custom Domain:** Configure and test domain if applicable

## Troubleshooting

### Build Fails
- Check build logs in Render dashboard
- Verify .NET 8.0 is specified in project file
- Ensure all NuGet packages restore correctly

### Email Not Working
- Verify Gmail App Password is correct
- Check environment variables are set properly
- Test SMTP settings with a simple email client

### Images Not Loading
- Verify images are in wwwroot/images directory
- Check case sensitivity of file names
- Ensure images are included in published output

## Render Free Tier Limitations

- **Automatic Sleep:** Service sleeps after 15 minutes of inactivity
- **Cold Starts:** May take 30+ seconds to wake up
- **750 Hours/Month:** Sufficient for most websites
- **Upgrade:** $7/month for always-on service

## Support

- **Render Documentation:** https://render.com/docs
- **JaxSun.us Issues:** Check project README for troubleshooting
- **Email Issues:** Verify Gmail security settings

---

## Quick Commands Summary

```bash
# Initialize git repository
git init
git add .
git commit -m "Initial commit"

# Add remote and push
git remote add origin https://github.com/yourusername/jaxsun-website.git
git push -u origin main

# Local testing
dotnet run --project JaxSun.Web
```

Your JaxSun.us website is now ready for production deployment on Render!