Introduction
=====

One of the really cool things about web projects is you can produce the .zip output by using the /t:Package target.  Of course, nothing in Visual Studio allows you to 'package' a solution containing web projects, so this is my initial attempt to do it.

Its components are:

* **panda.lib** - code to parse the Visual Studio solution file
* **panda.tasks** - MSBuild custom task to wrap the code
* **test.build** - MSBuild project file to orchestrate everything

A web project is any project template that would be hosted in IIS: ASP.NET applications and WCF Service applications.

The MSBuild script will take a solution file containing web projects and attempt to spit out a package folder containing all the results of **/t:Package** targets.

	C:\dev\panda.net>msbuild test.build /t:PackageSolution /p:Solutionfile=c:/dev/Services.sln
	
Assuming that the Services.sln contained two web projects (ServiceApp1.csproj and ServiceApp2.csproj), then the output product
should be:

	+ ./dev/Services_timestamp
	- ./dev/Services_timestamp/ServiceApp1.zip
	- ./dev/Services_timestamp/ServiceApp2.zip
	

The timestamp is simply a integer value that is generated for each package build.  

The output will also include the supporting output from /t:Package such as *.cmd and *.xml.


	
Sample MSBuild script
---------
	<!-- test.build -->
	<Project 
		ToolsVersion="4.0"
		xmlns="http://schemas.microsoft.com/developer/msbuild/2003"
		InitialTarget="PackageSolution">
  
	  <PropertyGroup>
		<!-- SolutionFile needs to be set on the commandline -->
	    <SolutionFile></SolutionFile>
	  </PropertyGroup>

	  <!-- external custom task to parse Visual Studio .sln file -->
	  <UsingTask 
		AssemblyFile="c:\temp\panda.tasks.dll"
		TaskName="GetProjects" 
		/>
  
	  <!-- 
		Generate a timestamp in the format of yyyymmddHHMMss.  
	
		I couldn't figure out how to do this inline because
		Now is a property and not a static method.  Its easy enough
		to wrap it, but I wouldn't want to do this as a general rule
		as the 'crust' just gets really deep.
	  -->
	  <UsingTask 
		TaskName="MakeTimeStamp" 
		TaskFactory="CodeTaskFactory"
		AssemblyFile="$(MSBuildToolsPath)\Microsoft.Build.Tasks.v4.0.dll">

	    <ParameterGroup>
	      <TimeStamp ParameterType="System.String" Output="true" />
	    </ParameterGroup>
	    <Task>
	      <Code Type="Fragment" Language="cs">TimeStamp =
	      System.DateTime.Now.ToString("yyyymmddHHMMss");</Code>
	    </Task>
	  </UsingTask>
  
	    <Target Name="PackageSolution">
			<!-- generate a value like 20115901130132 -->
			<MakeTimeStamp>
			  <Output PropertyName="T" TaskParameter="TimeStamp" />
			</MakeTimeStamp>
		
			<PropertyGroup>
				<SolutionName>$([System.IO.Path]::GetFileNameWithoutExtension($(SolutionFile)))</SolutionName>
				<SolutionPath>$([System.IO.Path]::GetDirectoryName($(SolutionFile)))</SolutionPath>
				<PackageFolder>$(SolutionPath)\$(SolutionName)_$(T)</PackageFolder>
			</PropertyGroup>
	
		<!--	
			<Message Text="Timestamp: $(T)"/>
			<Message Text="Solution File: $(SolutionFile)"/>
			<Message Text="Solution Path: $(SolutionPath)"/>	
			<Message Text="Solution Name: $(SolutionName)"/>	
			<Message Text="Package Folder: $(PackageFolder)"/>
		-->	
			<!-- grab all the project files in the solution -->
			<GetProjects SolutionPath="$(SolutionFile)">
			  <Output TaskParameter="Projects" ItemName="ProjectItems" />
			</GetProjects>
		
			<!-- 
				build everything using the /t:Package target and copy to the package folder 
		
				output:
				./solutionname_timestamp/projectname.zip
			-->
			<MSBuild 
				Projects="%(ProjectItems.ProjectPath)"
				RunEachTargetSeparately="true" 
				Targets="Package"
				Properties="PackageLocation=$(PackageFolder)/%(ProjectItems.Name).zip">
			</MSBuild>
		
		  </Target>
	</Project>
