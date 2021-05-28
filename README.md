# Mod Info
AutoRegister is a plugin created to automatically register new players on TShock servers requiring login. 

## Overview
### How it works
When a new user joins a server, AutoRegister checks to see if there is an existing user with their name/UUID in the database. If there isn't, a new user account is automatically created for the new player. The randomly generated password is then posted in the chat for them to save or change.

### Features
- Enables seamless onboarding of new players on login required servers. No more players getting confused and bombarding the host with "Why am I stoned/frozen/webbed/stuck?" questions.
- Automatically register a new account with a randomly generated password if an account by the player's name does not exist in the database.  
- Users will be notified in the chat of their newly AutoRegistered account and randomly generated password, which they can change later. 
- If an account by the same name as the new player already exist in the database, he/she will be notified in the chat to try a different username.
- Automatic registration success or failure is present in the same way as TShock for improved clarity and cohesion. 
- Writes to server console and logs if a new user was successfully registered. 
- Compatible with PC TShock 4.5.3

## Installation Guide
1. Copy and paste AutoRegister.dll into your ServerPlugins folder. That's it.
2. If the server is running, restart it. Otherwise start the server. 

## Source
[AutoRegister](https://tshock.co/xf/index.php?resources/autoregister.234/)
