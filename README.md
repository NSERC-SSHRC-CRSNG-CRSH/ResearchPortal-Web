# RP 2.0 Portal

RP 2.0 Portals is built on the xRM Portals Community Edition project.  The goal is to

## Objectives

It is the objective to limit the customizations, and make use of the configuratoins.  To accomplish this, we strive to not modify the underlaying MasterPortal project, but build a layer to runs ontop of it.

If there are features and customizations that the core xRM Portals would benifit from, it would be in the interest in contributing the changes to the core framework back to the project.

## Building

To build the project, ensure that you have [Git](https://git-scm.com/downloads) installed to obtain the source code, and [Visual Studio 2017](https://docs.microsoft.com/en-us/visualstudio/welcome-to-visual-studio) installed to compile the source code.

Clone the RP 2.0 project, and initialze you it.

Since we cannot modify the underlying xRM Portals project, it must be
```ps
.\PortalsBuildScripts\build.ps1 -command InitializeIIS
```

## Deployment

xRM Portals Community Edition is a set of .NET class libraries and an ASP.NET web application called `MasterPortal`. After building the project, `MasterPortal` is run using conventional ASP.NET website hosting methods such as using [IIS](https://www.iis.net/) in on-premise environments, and [Azure Web Apps](https://docs.microsoft.com/en-ca/azure/app-service-web/app-service-web-overview) in cloud environments.

The `MasterPortal` web application  deployment is dependent upon schema (solutions) and data being installed in a Dynamics 365 instance. These components are downloaded from the [Microsoft Download Center](https://www.microsoft.com/en-us/download/details.aspx?id=55789) in the file `MicrosoftDynamics365PortalsSolutions.exe`. The components in this download have not been released under the MIT license and are not managed by the xRM Portals Community Edition project.

A full description of the deployment process is described in the file `Self-hosted_Installation_Guide_for_Portals.pdf` available for download on the [Microsoft Download Center](https://www.microsoft.com/en-us/download/details.aspx?id=55789).

## System Requirements

The following system requirements are additional to those listed in `Self-hosted_Installation_Guide_for_Portals.pdf`:

- The website must be set to run in 64-bit mode:

  IIS Application Pool:
   
  ![image](https://user-images.githubusercontent.com/10599498/30821566-03ec5466-a1e3-11e7-80bd-bb0b1c724452.png)

  Azure Web App:
   
  ![image](https://user-images.githubusercontent.com/10599498/30821633-468576ae-a1e3-11e7-8b45-e55df1742629.png)

- IIS 7.5 (Windows 7 or Windows Server 2008 R2) requires the installation of the [IIS Application Initialization module](https://www.iis.net/downloads/microsoft/application-initialization). Use the `x64` download link at the [bottom of the page](https://www.iis.net/downloads/microsoft/application-initialization#additionalDownloads).

## Support

There are two primary methods of obtaining support for this project:

1. Community-driven support is available by [submitting issues](https://github.com/Adoxio/xRM-Portals-Community-Edition/issues) to this GitHub project
2. Commercial support options are available from [Adoxio](https://www.adoxio.com/xRM-Portals-Community-Edition/)

## License

This project uses the [MIT license](https://opensource.org/licenses/MIT).

## Contributions

This project accepts community contributions through GitHub, following the [inbound=outbound](https://opensource.guide/legal/#does-my-project-need-an-additional-contributor-agreement) model as described in the [GitHub Terms of Service](https://help.github.com/articles/github-terms-of-service/#6-contributions-under-repository-license):
> Whenever you make a contribution to a repository containing notice of a license, you license your contribution under the same terms, and you agree that you have the right to license your contribution under those terms.

Please submit one pull request per issue so that we can easily identify and review the changes.
