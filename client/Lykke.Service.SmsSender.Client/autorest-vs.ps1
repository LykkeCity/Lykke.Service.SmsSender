# This script uses Autorest to generate service's client library

# == Prerequisites ==

# Nodejs version >= 6.11.2 - https://nodejs.org/en/download/
# NPM version >= 3.10.10 - https://www.npmjs.com/get-npm
# Autorest version >= 1.2.2 - https://www.npmjs.com/package/autorest

# You can use https://github.com/Azure/autorest/releases/download/2.0.4222/Autorest.Windows.Portable.2.0.4222.zip
# if you have any problems running autorest 1.2.2.
# Just unpack archive in this directory and run ./autorest.cmd instead of 
# just autorest

# Run this file if your use Execute as Script command of Visual Studio's PowerShell Tools extension
autorest --input-file=http://localhost:5000/swagger/v1/swagger.json --csharp --output-folder=./client/Lykke.Service.SmsSender.Client/AutorestClient --namespace=Lykke.Service.SmsSender.Client.AutorestClient