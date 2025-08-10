# AWS Elastic Beanstalk Deployment Guide

## Pre-Deployment Setup Complete ✅

Your Yopo Backend API is now configured for AWS Elastic Beanstalk deployment with automatic database migration.

## Configuration Summary

### 🎯 Environment Configuration
- **Target Framework:** .NET 9
- **Environment:** Production
- **Platform:** 64bit Amazon Linux 2023 v3.5.3 running .NET 9
- **Instance Type:** t3.micro (can be changed)

### 🗄️ Database Configuration
- **Database:** MySQL (AWS RDS)
- **Username:** yopoapidb
- **Password:** yopoapidb
- **Database Name:** ebdb
- **Endpoint:** awseb-e-wwvqxp9xff-stack-awsebrdsdatabase-65cz859dcjb3.ci5sgygui6pi.us-east-1.rds.amazonaws.com:3306
- **SSL:** Required

### 🚀 Deployment Features
- ✅ Automatic database migrations on startup
- ✅ Production environment configuration
- ✅ JWT authentication with environment variables
- ✅ SSL/TLS support for database connections
- ✅ IIS hosting configuration
- ✅ Static file serving
- ✅ Swagger documentation (accessible at `/swagger`)

## Files Created/Modified

### 📁 Configuration Files
- `appsettings.Production.json` - Production database and app configuration
- `.ebextensions/01-environment.config` - AWS environment variables
- `.ebextensions/02-iis-config.config` - IIS and load balancer settings
- `web.config` - IIS hosting configuration
- `aws-beanstalk-tools-defaults.json` - AWS Toolkit deployment settings

### 📁 Project Files
- `YopoBackend.csproj` - Updated for .NET 9 and AWS deployment
- `Program.cs` - Modified for automatic migration support
- `wwwroot/index_production.html` - Simple production landing page

## How to Deploy Using Visual Studio 2022 AWS Toolkit

### 1. Prerequisites
- Visual Studio 2022 with AWS Toolkit extension installed
- AWS credentials configured in Visual Studio
- AWS account with Elastic Beanstalk access

### 2. Deployment Steps

1. **Open the project** in Visual Studio 2022
2. **Right-click on the project** in Solution Explorer
3. **Select "Publish to AWS Elastic Beanstalk..."**
4. **Choose your existing environment:**
   - Application Name: `yopo-backend`
   - Environment Name: `yopobackend-env`
   - Region: `us-east-1`

5. **Verify the settings match:**
   - Platform: `64bit Amazon Linux 2023 v3.5.3 running .NET 9`
   - Instance Type: `t3.micro` (or as desired)
   - Configuration: `Release`

6. **Click "Publish"**

The AWS Toolkit will:
- Build the project in Release mode
- Package the application with all configuration files
- Upload to AWS Elastic Beanstalk
- Deploy and start the application

### 3. Post-Deployment Verification

After deployment, your API will be available at:
`http://yopobackend-env.eba-5btzurmb.us-east-1.elasticbeanstalk.com/`

**Test these endpoints:**
- **Root:** `http://yopobackend-env.eba-5btzurmb.us-east-1.elasticbeanstalk.com/` (Landing page)
- **API Docs:** `http://yopobackend-env.eba-5btzurmb.us-east-1.elasticbeanstalk.com/swagger`
- **Modules:** `http://yopobackend-env.eba-5btzurmb.us-east-1.elasticbeanstalk.com/api/modules`

## Automatic Features on Deployment

### 🔧 Database Migration
- **Development:** Uses `EnsureCreated()` for simplicity
- **Production:** Uses `MigrateAsync()` for proper schema management
- **Auto-Migration:** Runs automatically on application startup
- **Error Handling:** Comprehensive logging for migration issues

### 🔐 Environment Variables
The following environment variables are automatically set:
```
ASPNETCORE_ENVIRONMENT=Production
yopo_api_serve=Production
MYSQL_CONNECTION_STRING=Server=awseb-e-wwvqxp9xff-stack-awsebrdsdatabase-65cz859dcjb3.ci5sgygui6pi.us-east-1.rds.amazonaws.com;Port=3306;Database=ebdb;Uid=yopoapidb;Pwd=yopoapidb;SslMode=Required;
JWT_SECRET_KEY=YourSuperSecretJWTKeyThatShouldBeAtLeast64CharactersLongForBetterSecurity123456789
```

## Monitoring and Troubleshooting

### 📊 Application Logs
- Access logs through AWS Elastic Beanstalk console
- Navigate to: Environment → Logs → Request Logs

### 🛠️ Common Issues and Solutions

**Migration Failures:**
- Check database connectivity in AWS RDS console
- Verify security groups allow connection from Elastic Beanstalk
- Review application logs for detailed error messages

**Performance Issues:**
- Consider upgrading from t3.micro to t3.small for better performance
- Enable Application Load Balancer if needed
- Monitor CloudWatch metrics

**SSL/TLS Issues:**
- Ensure RDS instance has SSL enabled
- Check security group configurations
- Verify connection string includes `SslMode=Required`

## Security Considerations

### 🔒 Production Security
- Database credentials are configured via environment variables
- JWT secret key is set via environment variables
- SSL/TLS enforced for database connections
- Rate limiting enabled for production environment

### 🛡️ Recommendations
1. **Update JWT Secret:** Change the JWT secret key to a unique value
2. **Database Security:** Consider rotating database passwords regularly
3. **Monitoring:** Enable CloudWatch monitoring for security events
4. **Backup:** Ensure automated backups are configured for RDS

## Next Steps

After successful deployment:

1. **Test all API endpoints** using the Swagger documentation
2. **Create initial user accounts** through the admin panel
3. **Configure monitoring** and alerts in AWS CloudWatch
4. **Set up automated deployments** using AWS CodePipeline (optional)
5. **Configure custom domain** and SSL certificate (optional)

## Support

If you encounter any issues during deployment:
1. Check the AWS Elastic Beanstalk health dashboard
2. Review application logs in the AWS console
3. Verify all environment variables are set correctly
4. Ensure the RDS database is accessible from the Elastic Beanstalk environment

---

**✨ Your Yopo Backend API is now ready for AWS deployment with automatic database migration!**
