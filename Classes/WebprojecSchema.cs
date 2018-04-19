using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroserviceExplorer.Classes.web
{



// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Project
    {

        private ProjectPropertyGroup[] propertyGroupField;

        private ProjectItemGroup[] itemGroupField;

        private ProjectUsingTask[] usingTaskField;

        private ProjectTarget[] targetField;

        private string sdkField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PropertyGroup")]
        public ProjectPropertyGroup[] PropertyGroup
        {
            get { return this.propertyGroupField; }
            set { this.propertyGroupField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemGroup")]
        public ProjectItemGroup[] ItemGroup
        {
            get { return this.itemGroupField; }
            set { this.itemGroupField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("UsingTask")]
        public ProjectUsingTask[] UsingTask
        {
            get { return this.usingTaskField; }
            set { this.usingTaskField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Target")]
        public ProjectTarget[] Target
        {
            get { return this.targetField; }
            set { this.targetField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Sdk
        {
            get { return this.sdkField; }
            set { this.sdkField = value; }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectPropertyGroup
    {

        private string noWarnField;

        private string outputPathField;

        private string targetFrameworkField;

        private decimal typeScriptToolsVersionField;

        private bool typeScriptToolsVersionFieldSpecified;

        private bool copyLocalLockFileAssembliesField;

        private bool copyLocalLockFileAssembliesFieldSpecified;

        private string conditionField;

        /// <remarks/>
        public string NoWarn
        {
            get { return this.noWarnField; }
            set { this.noWarnField = value; }
        }

        /// <remarks/>
        public string OutputPath
        {
            get { return this.outputPathField; }
            set { this.outputPathField = value; }
        }

        /// <remarks/>
        public string TargetFramework
        {
            get { return this.targetFrameworkField; }
            set { this.targetFrameworkField = value; }
        }

        /// <remarks/>
        public decimal TypeScriptToolsVersion
        {
            get { return this.typeScriptToolsVersionField; }
            set { this.typeScriptToolsVersionField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TypeScriptToolsVersionSpecified
        {
            get { return this.typeScriptToolsVersionFieldSpecified; }
            set { this.typeScriptToolsVersionFieldSpecified = value; }
        }

        /// <remarks/>
        public bool CopyLocalLockFileAssemblies
        {
            get { return this.copyLocalLockFileAssembliesField; }
            set { this.copyLocalLockFileAssembliesField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CopyLocalLockFileAssembliesSpecified
        {
            get { return this.copyLocalLockFileAssembliesFieldSpecified; }
            set { this.copyLocalLockFileAssembliesFieldSpecified = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Condition
        {
            get { return this.conditionField; }
            set { this.conditionField = value; }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroup
    {

        private ProjectItemGroupProjectReference projectReferenceField;

        private ProjectItemGroupPackageReference[] packageReferenceField;

        private ProjectItemGroupDotNetCliToolReference dotNetCliToolReferenceField;

        private ProjectItemGroupCompile compileField;

        private ProjectItemGroupContent contentField;

        private ProjectItemGroupEmbeddedResource embeddedResourceField;

        private ProjectItemGroupNone noneField;

        /// <remarks/>
        public ProjectItemGroupProjectReference ProjectReference
        {
            get { return this.projectReferenceField; }
            set { this.projectReferenceField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PackageReference")]
        public ProjectItemGroupPackageReference[] PackageReference
        {
            get { return this.packageReferenceField; }
            set { this.packageReferenceField = value; }
        }

        /// <remarks/>
        public ProjectItemGroupDotNetCliToolReference DotNetCliToolReference
        {
            get { return this.dotNetCliToolReferenceField; }
            set { this.dotNetCliToolReferenceField = value; }
        }

        /// <remarks/>
        public ProjectItemGroupCompile Compile
        {
            get { return this.compileField; }
            set { this.compileField = value; }
        }

        /// <remarks/>
        public ProjectItemGroupContent Content
        {
            get { return this.contentField; }
            set { this.contentField = value; }
        }

        /// <remarks/>
        public ProjectItemGroupEmbeddedResource EmbeddedResource
        {
            get { return this.embeddedResourceField; }
            set { this.embeddedResourceField = value; }
        }

        /// <remarks/>
        public ProjectItemGroupNone None
        {
            get { return this.noneField; }
            set { this.noneField = value; }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupProjectReference
    {

        private string includeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Include
        {
            get { return this.includeField; }
            set { this.includeField = value; }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupPackageReference
    {

        private string includeField;

        private string versionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Include
        {
            get { return this.includeField; }
            set { this.includeField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version
        {
            get { return this.versionField; }
            set { this.versionField = value; }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupDotNetCliToolReference
    {

        private string includeField;

        private string versionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Include
        {
            get { return this.includeField; }
            set { this.includeField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version
        {
            get { return this.versionField; }
            set { this.versionField = value; }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupCompile
    {

        private string removeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Remove
        {
            get { return this.removeField; }
            set { this.removeField = value; }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupContent
    {

        private string removeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Remove
        {
            get { return this.removeField; }
            set { this.removeField = value; }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupEmbeddedResource
    {

        private string removeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Remove
        {
            get { return this.removeField; }
            set { this.removeField = value; }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupNone
    {

        private string removeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Remove
        {
            get { return this.removeField; }
            set { this.removeField = value; }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectUsingTask
    {

        private string assemblyFileField;

        private string taskNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AssemblyFile
        {
            get { return this.assemblyFileField; }
            set { this.assemblyFileField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TaskName
        {
            get { return this.taskNameField; }
            set { this.taskNameField = value; }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectTarget
    {

        private ProjectTargetWebCompilerCompilerCleanTask webCompilerCompilerCleanTaskField;

        private ProjectTargetWebCompilerCompilerBuildTask webCompilerCompilerBuildTaskField;

        private string nameField;

        private string afterTargetsField;

        private string conditionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("WebCompiler.CompilerCleanTask")]
        public ProjectTargetWebCompilerCompilerCleanTask WebCompilerCompilerCleanTask
        {
            get { return this.webCompilerCompilerCleanTaskField; }
            set { this.webCompilerCompilerCleanTaskField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("WebCompiler.CompilerBuildTask")]
        public ProjectTargetWebCompilerCompilerBuildTask WebCompilerCompilerBuildTask
        {
            get { return this.webCompilerCompilerBuildTaskField; }
            set { this.webCompilerCompilerBuildTaskField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AfterTargets
        {
            get { return this.afterTargetsField; }
            set { this.afterTargetsField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Condition
        {
            get { return this.conditionField; }
            set { this.conditionField = value; }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectTargetWebCompilerCompilerCleanTask
    {

        private bool continueOnErrorField;

        private string fileNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool ContinueOnError
        {
            get { return this.continueOnErrorField; }
            set { this.continueOnErrorField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FileName
        {
            get { return this.fileNameField; }
            set { this.fileNameField = value; }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectTargetWebCompilerCompilerBuildTask
    {

        private bool continueOnErrorField;

        private string fileNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool ContinueOnError
        {
            get { return this.continueOnErrorField; }
            set { this.continueOnErrorField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FileName
        {
            get { return this.fileNameField; }
            set { this.fileNameField = value; }
        }
    }

}