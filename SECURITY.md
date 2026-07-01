# Security Policy

## Reporting Security Vulnerabilities

If you discover a security vulnerability in UPEE, please **DO NOT** open a public GitHub issue.
Instead, email your findings to: **security@upee-engine.dev** (or use responsible disclosure practices).

### What to Include
- Description of the vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (if any)

We will respond within **48 hours** and work with you to verify and resolve the issue.

## Security Features

UPEE implements the following security measures:

1. **Process Sandboxing**: All scripts run in isolated processes with configurable resource limits
2. **Directory Isolation**: Scripts can only access configured workspace directories
3. **Timeout Enforcement**: Runaway scripts are forcibly terminated after configured duration
4. **Audit Logging**: All executions are logged with timestamps and status
5. **Data Bus Encryption-Ready**: UniversalDataBus supports encrypted payloads for sensitive data

## Known Vulnerabilities

- **System.Text.Json (v8.0.1)**: Advisory NU1903 - Consider upgrading to latest patch version
- **Windows-Only APIs**: CA1416 warnings for EventLog on non-Windows platforms (expected behavior)

## Recommended Practices

1. **Run UPEE in Trusted Environments**: Only execute scripts from trusted sources
2. **Use Configuration Limits**: Set appropriate timeouts and concurrent task limits in upee.config.json
3. **Monitor Logs**: Review history.log regularly for suspicious activity
4. **Keep Updated**: Install latest patches and monitor security advisories
5. **Restrict File Access**: Configure workspace directories to limit script scope

## Security Updates

Security updates will be released as patches (e.g., 1.0.1, 1.0.2) and communicated to users promptly.
Subscribe to GitHub releases to stay informed.

## Additional Resources

- [OWASP Top 10](https://owasp.org/Top10/)
- [CWE/SANS Top 25](https://cwe.mitre.org/top25/)
- [.NET Security Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/security/)
