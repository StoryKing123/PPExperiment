using PPConcurrentTest.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLogging();  // 确保日志服务已注册
builder.Services.AddSingleton<DataverseClientService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
   var dataverseService = scope.ServiceProvider.GetRequiredService<DataverseClientService>();
   // 服务已经被实例化，构造函数中的初始化代码会被执行
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
