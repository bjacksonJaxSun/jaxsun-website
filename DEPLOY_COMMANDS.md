# JaxSun.us Deployment Commands

## After Creating GitHub Repository

Run these commands to push your code:

```bash
cd C:\Development\JaxSun.us
git remote add origin https://github.com/bjackson071968/jaxsun-website.git
git push -u origin main
```

## Deployment API Call Ready

Once the repository exists, I'll run this deployment command:

```bash
curl -X POST "https://api.render.com/v1/services" \
  -H "Authorization: Bearer rnd_5mVOfiXNkStrffb4mVp8zT1qQNpX" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "web_service",
    "ownerId": "tea-d1k8evfdiees73e6hntg",
    "name": "jaxsun-website", 
    "repo": "https://github.com/bjackson071968/jaxsun-website",
    "branch": "main",
    "serviceDetails": {
      "env": "docker",
      "plan": "starter", 
      "region": "oregon",
      "envSpecificDetails": {
        "dockerfilePath": "./Dockerfile"
      }
    },
    "envVars": [
      {"key": "ASPNETCORE_ENVIRONMENT", "value": "Production"},
      {"key": "ASPNETCORE_URLS", "value": "http://0.0.0.0:10000"},
      {"key": "EmailSettings__SmtpServer", "value": "smtp.gmail.com"},
      {"key": "EmailSettings__SmtpPort", "value": "587"},
      {"key": "EmailSettings__FromEmail", "value": "contact@jaxsun.us"},
      {"key": "EmailSettings__FromName", "value": "JaxSun Partnership"},
      {"key": "EmailSettings__Username", "value": "bjackson071968@gmail.com"},
      {"key": "EmailSettings__Password", "value": "qukf vkft xwnl lign"}
    ]
  }'
```

This will deploy your JaxSun.us website to Render automatically!