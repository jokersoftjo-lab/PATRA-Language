using Patra.Core;

if (args.Length != 1)
{
    Console.Error.WriteLine("الاستخدام: patra <مسار-ملف.patra>");
    return 1;
}

try
{
    string path = Path.GetFullPath(args[0]);
    string source = File.ReadAllText(path);
    var interpreter = new Interpreter();

    foreach (string line in interpreter.Execute(source))
        Console.WriteLine(line);

    return 0;
}
catch (LexingException ex)
{
    Console.Error.WriteLine($"خطأ معجمي: {ex.Message}");
    return 2;
}
catch (SyntaxException ex)
{
    Console.Error.WriteLine($"خطأ نحوي: {ex.Message}");
    return 2;
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"خطأ أثناء التنفيذ: {ex.Message}");
    return 3;
}
catch (IOException ex)
{
    Console.Error.WriteLine($"تعذر قراءة الملف: {ex.Message}");
    return 4;
}
catch (UnauthorizedAccessException ex)
{
    Console.Error.WriteLine($"لا توجد صلاحية للوصول إلى الملف: {ex.Message}");
    return 4;
}
