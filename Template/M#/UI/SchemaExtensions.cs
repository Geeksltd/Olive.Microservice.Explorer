// ********************************************************************
// WARNING: This file is auto-generated from M# Model.
// and may be overwritten at any time. Do not change it manually.
// ********************************************************************

namespace MSharp
{
    using System.Runtime.CompilerServices;
    
    static partial class SchemaExtensions
    {
        [MethodColor("#afcd14")]
        public static PropertyFilterElement<Domain.BackgroundTask> Name(this ListModule<Domain.BackgroundTask>.SearchComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Search(x => x.Name, file, line);
        
        [MethodColor("#afcd14")]
        public static PropertyFilterElement<Domain.BackgroundTask> ExecutingInstance(this ListModule<Domain.BackgroundTask>.SearchComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Search(x => x.ExecutingInstance, file, line);
        
        [MethodColor("#afcd14")]
        public static PropertyFilterElement<Domain.BackgroundTask> Heartbeat(this ListModule<Domain.BackgroundTask>.SearchComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Search(x => x.Heartbeat, file, line);
        
        [MethodColor("#afcd14")]
        public static PropertyFilterElement<Domain.BackgroundTask> LastExecuted(this ListModule<Domain.BackgroundTask>.SearchComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Search(x => x.LastExecuted, file, line);
        
        [MethodColor("#afcd14")]
        public static PropertyFilterElement<Domain.BackgroundTask> IntervalInMinutes(this ListModule<Domain.BackgroundTask>.SearchComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Search(x => x.IntervalInMinutes, file, line);
        
        [MethodColor("#afcd14")]
        public static PropertyFilterElement<Domain.BackgroundTask> TimeoutInMinutes(this ListModule<Domain.BackgroundTask>.SearchComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Search(x => x.TimeoutInMinutes, file, line);
        
        [MethodColor("#afcd14")]
        public static ViewElement<Domain.BackgroundTask> Name(this ViewModule<Domain.BackgroundTask>.FieldComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Field(x => x.Name, file, line);
        
        [MethodColor("#afcd14")]
        public static ViewElement<Domain.BackgroundTask> ExecutingInstance(this ViewModule<Domain.BackgroundTask>.FieldComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Field(x => x.ExecutingInstance, file, line);
        
        [MethodColor("#afcd14")]
        public static ViewElement<Domain.BackgroundTask> Heartbeat(this ViewModule<Domain.BackgroundTask>.FieldComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Field(x => x.Heartbeat, file, line);
        
        [MethodColor("#afcd14")]
        public static ViewElement<Domain.BackgroundTask> LastExecuted(this ViewModule<Domain.BackgroundTask>.FieldComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Field(x => x.LastExecuted, file, line);
        
        [MethodColor("#afcd14")]
        public static ViewElement<Domain.BackgroundTask> IntervalInMinutes(this ViewModule<Domain.BackgroundTask>.FieldComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Field(x => x.IntervalInMinutes, file, line);
        
        [MethodColor("#afcd14")]
        public static ViewElement<Domain.BackgroundTask> TimeoutInMinutes(this ViewModule<Domain.BackgroundTask>.FieldComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Field(x => x.TimeoutInMinutes, file, line);
        
        [MethodColor("#0ccc68")]
        public static ViewElement<Domain.BackgroundTask> Name(this ListModule<Domain.BackgroundTask>.ColumnComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Column(x => x.Name, file, line);
        
        [MethodColor("#0ccc68")]
        public static ViewElement<Domain.BackgroundTask> ExecutingInstance(this ListModule<Domain.BackgroundTask>.ColumnComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Column(x => x.ExecutingInstance, file, line);
        
        [MethodColor("#0ccc68")]
        public static ViewElement<Domain.BackgroundTask> Heartbeat(this ListModule<Domain.BackgroundTask>.ColumnComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Column(x => x.Heartbeat, file, line);
        
        [MethodColor("#0ccc68")]
        public static ViewElement<Domain.BackgroundTask> LastExecuted(this ListModule<Domain.BackgroundTask>.ColumnComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Column(x => x.LastExecuted, file, line);
        
        [MethodColor("#0ccc68")]
        public static ViewElement<Domain.BackgroundTask> IntervalInMinutes(this ListModule<Domain.BackgroundTask>.ColumnComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Column(x => x.IntervalInMinutes, file, line);
        
        [MethodColor("#0ccc68")]
        public static ViewElement<Domain.BackgroundTask> TimeoutInMinutes(this ListModule<Domain.BackgroundTask>.ColumnComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Column(x => x.TimeoutInMinutes, file, line);
        
        [MethodColor("#AFCD14")]
        public static StringFormElement Name(this FormModule<Domain.BackgroundTask>.FieldComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Field(x => x.Name, file, line);
        
        [MethodColor("#AFCD14")]
        public static StringFormElement ExecutingInstance(this FormModule<Domain.BackgroundTask>.FieldComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Field(x => x.ExecutingInstance, file, line);
        
        [MethodColor("#AFCD14")]
        public static DateTimeFormElement Heartbeat(this FormModule<Domain.BackgroundTask>.FieldComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Field(x => x.Heartbeat, file, line);
        
        [MethodColor("#AFCD14")]
        public static DateTimeFormElement LastExecuted(this FormModule<Domain.BackgroundTask>.FieldComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Field(x => x.LastExecuted, file, line);
        
        [MethodColor("#AFCD14")]
        public static NumberFormElement IntervalInMinutes(this FormModule<Domain.BackgroundTask>.FieldComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Field(x => x.IntervalInMinutes, file, line);
        
        [MethodColor("#AFCD14")]
        public static NumberFormElement TimeoutInMinutes(this FormModule<Domain.BackgroundTask>.FieldComponents @this, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) => @this.module.Field(x => x.TimeoutInMinutes, file, line);
    }
}