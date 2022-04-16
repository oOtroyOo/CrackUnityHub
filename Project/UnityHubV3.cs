﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CrackUnityHub
{
    internal class UnityHubV3 : Patcher
    {
        const string init = @"init() {
        return __awaiter(this, void 0, void 0, function* () {
            this.authLogger.info('Initializing the auth service');
            this.initNetworkInterceptors();
            electron_1.powerMonitor.on('resume', () => {
                this.authLogger.debug('Resetting token watcher timeout after system resume');
                this.monitorTokens();
            });
            this.onNetworkUp();
            this.logInWithAccessToken();
            return this.userInfo;
        });
    }";
        const string openSignIn = @"openSignIn() {
        this.logInWithAccessToken();
    }";
        const string logInWithAccessToken = @"logInWithAccessToken(accessToken) {
        return __awaiter(this, void 0, void 0, function* () {
            try {
                this.authLogger.info('Fetching user info from the identity provider using access token');
                this.userInfo = this.getFormattedUserInfo();
                this.emit(PostalTopic_1.default.USER_INFO_UPDATED, this.userInfo);
                this.setLoggedInFlags();
                this.monitorTokens();
                this.emit(AuthEvents_1.default.LOGGED_IN_WITH_ACCESS_TOKEN, undefined);
                return this.userInfo;
            }
            catch (error) {
                this.authLogger.error(`Error fetching user info from Access Token: ${error}`);
                HeapService_1.default.sendSignInErrorEvent('logInWithAccessToken');
                this.setLoggedOutFlags();
                throw error;
            }
        });
    }";
        const string isTokenValid = @"static isTokenValid(token) {
        return true;
    }";

        const string getValidEntitlementGroups = @"getValidEntitlementGroups() {
        return __awaiter(this, void 0, void 0, function* () {
            return [{
                startDate: new Date('1993-01-01T08:00:00.000Z'),
                expirationDate: new Date('9999-01-01T08:00:00.000Z'),
                productName: i18nHelper_1.default.i18n.translate('license-management:TYPE_PRO'),
                licenseType: 'PRO',
            }];
        });
    }";

        const string isLicenseValid = @"isLicenseValid() {
        return __awaiter(this, void 0, void 0, function* () {
            return true;
        });
    }";

        public override bool IsMatch(string version)
        {
            return version.StartsWith("3.");
        }

        public override bool Patch(string exportFolder)
        {
            var authServicePath = Path.Combine(exportFolder, "build/main/services/authService/AuthService.js");
            var authServiceContent = File.ReadAllText(authServicePath);
            ReplaceMethod(ref authServiceContent, @"init\(\)\s*", init);
            ReplaceMethod(ref authServiceContent, @"openSignIn\(\)\s*", openSignIn);
            ReplaceMethod(ref authServiceContent, @"logInWithAccessToken\(accessToken\)\s*", logInWithAccessToken);
            ReplaceMethod(ref authServiceContent, @"static\sisTokenValid\(token\)\s*", isTokenValid);
            File.WriteAllText(authServicePath, authServiceContent);

            var licenseServicePath = Path.Combine(exportFolder, "build/main/services/licenseService/licenseService.js");
            var licenseServiceContent = File.ReadAllText(licenseServicePath);
            ReplaceMethod(ref licenseServiceContent, @"getValidEntitlementGroups\(\)\s*", getValidEntitlementGroups);
            ReplaceMethod(ref licenseServiceContent, @"isLicenseValid\(\)\s*", isLicenseValid);
            File.WriteAllText(licenseServicePath, licenseServiceContent);

            return true;
        }
    }
}
