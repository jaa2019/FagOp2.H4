using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using WebAPI.DAL;
using WebAPI.Models;
using Xunit;

namespace WebAPITest
{
    public class TodoTests
    {
        [Fact]
        public async Task GetTodos()
        {
            await using var application = new TodoApplication();

            var client = application.CreateClient();
            var todos = await client.GetFromJsonAsync<List<Todo>>("/todos");

            Assert.Empty(todos);
        }

        [Fact]
        public async Task PostTodos()
        {
            await using var application = new TodoApplication();

            var client = application.CreateClient();
            var response =
                await client.PostAsJsonAsync("/todos", new Todo { Description = "I want to do this thing tomorrow" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var todos = await client.GetFromJsonAsync<List<Todo>>("/todos");

            var todo = Assert.Single(todos);
            Assert.Equal("I want to do this thing tomorrow", todo.Description);
            Assert.False(todo.Completed);
        }

        [Fact]
        public async Task DeleteTodos()
        {
            await using var application = new TodoApplication();

            var client = application.CreateClient();
            var response =
                await client.PostAsJsonAsync("/todos", new Todo { Description = "I want to do this thing tomorrow" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var todos = await client.GetFromJsonAsync<List<Todo>>("/todos");

            var todo = Assert.Single(todos);
            Assert.Equal("I want to do this thing tomorrow", todo.Description);
            Assert.False(todo.Completed);

            response = await client.DeleteAsync($"/todos/{todo.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await client.GetAsync($"/todos/{todo.Id}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    class TodoApplication : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            var root = new InMemoryDatabaseRoot();

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<TodoDb>));

                services.AddDbContext<TodoDb>(options =>
                    options.UseInMemoryDatabase("Testing", root));
            });

            return base.CreateHost(builder);
        }
    }
}