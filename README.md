# Email/Service Ticket Translation

## Overview
A .NET App I created in 2020 for the purpose of monitoring an email inbox, translating the emails to English, then sending to another inbox.
This was for the purpose of allowing non English speaking customers to raise service desk tickets with our IT team.


I used for scheduling mailbox scans and the Google Translate library to provide the translations.

## Features

- Automated Monitoring: Utilizes Hangfire for scheduling regular scans of the mailbox, ensuring timely processing of incoming emails.

- Seamless Translation: Leverages the Google Translate library to seamlessly translate emails into English, facilitating smooth communication between users and the IT team.

- Enhanced Accessibility: Empowers non-English-speaking customers to effortlessly raise service desk tickets, promoting inclusivity and accessibility within the organization.

## Technologies Used

 - [Hangfire](https://www.hangfire.io/): Employed for efficient scheduling of mailbox scans, ensuring prompt handling of incoming emails.

 - [Google Translate Library](https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Translate.V3/latest): Integrated to provide accurate and reliable translations, bridging language barriers effectively.

