# NotesCool CentOS 9 systemd environment configuration

This runbook configures NotesCool so the backend container receives auth-related environment variables from the CentOS 9 system service instead of a repository `.env` file.

## Files

- Systemd unit template: `deploy/systemd/notescool.service`
- Environment template: `deploy/systemd/notescool.env.example`

## Install

Run as root or with `sudo` on the CentOS 9 server.

```bash
# 1. Copy the systemd unit
cp deploy/systemd/notescool.service /etc/systemd/system/notescool.service

# 2. Create the sysconfig environment file from the template
cp deploy/systemd/notescool.env.example /etc/sysconfig/notescool
chmod 600 /etc/sysconfig/notescool

# 3. Edit real values on the server only
vi /etc/sysconfig/notescool

# 4. Reload and start the service
systemctl daemon-reload
systemctl enable notescool.service
systemctl restart notescool.service

# 5. Verify
systemctl status notescool.service --no-pager
docker compose ps
```

## Required auth environment variables

The backend uses ASP.NET Core configuration binding, where `__` maps to `:`.

### Google provider

```bash
SSO__PROVIDERS__0__NAME=Google
SSO__PROVIDERS__0__ENABLED=true
SSO__PROVIDERS__0__CLIENTID=<google-client-id>
SSO__PROVIDERS__0__CLIENTSECRET=<google-client-secret>
SSO__PROVIDERS__0__AUTHORITY=https://accounts.google.com
SSO__PROVIDERS__0__CALLBACKPATH=/signin-google
SSO__PROVIDERS__0__REDIRECTURLS__0=https://your-frontend-domain/auth/callback/google
```

### Microsoft provider

```bash
SSO__PROVIDERS__1__NAME=Microsoft
SSO__PROVIDERS__1__ENABLED=false
SSO__PROVIDERS__1__CLIENTID=<microsoft-client-id>
SSO__PROVIDERS__1__CLIENTSECRET=<microsoft-client-secret>
SSO__PROVIDERS__1__AUTHORITY=https://login.microsoftonline.com/common/v2.0
SSO__PROVIDERS__1__CALLBACKPATH=/signin-microsoft
SSO__PROVIDERS__1__REDIRECTURLS__0=https://your-frontend-domain/auth/callback/microsoft
```

## Applying client id/secret changes

Do not edit `appsettings.json` and do not create a repository `.env` file.

Update `/etc/sysconfig/notescool`, then restart the service:

```bash
systemctl restart notescool.service
systemctl status notescool.service --no-pager
```

Restarting the service recreates the containers through Docker Compose, so the backend receives the updated environment variables.

## Rollback

```bash
systemctl stop notescool.service
cp /etc/systemd/system/notescool.service.bak /etc/systemd/system/notescool.service
systemctl daemon-reload
systemctl restart notescool.service
```

If only environment values are wrong, restore the previous `/etc/sysconfig/notescool` backup and restart the service.
