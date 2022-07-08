using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserManagement.Data;
using UserManagement.Models;

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManagementContext _context;
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionGroupCollectionProvider;

        public UsersController(UserManagementContext context, IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider)
        {
            _context = context;
            _apiDescriptionGroupCollectionProvider = apiDescriptionGroupCollectionProvider;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
          if (_context.Users == null)
          {
              return NotFound();
          }
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(long id)
        {
          if (_context.Users == null)
          {
              return NotFound();
          }
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(long id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // Small change
        [Produces("application/json")]
        [Consumes("application/json")]
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
          if (_context.Users == null)
          {
              return Problem("Entity set 'UserManagementContext.Users'  is null.");
          }
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(long id)
        {
            return (_context.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
        }

        // POST: api/Users
        [HttpPost]
        [Route("Guidance/{useCase?}")]
        public async Task<ActionResult<string>> PostUserHint( [FromBody] UserNotValidated user, [FromRoute] UseCase? useCase = UseCase.Default)
        {
            var openApiDocument = GetBasicOpenApiDocument();
            FilterToPathAndOperation("/api/users", OperationType.Post, ref openApiDocument);
            FilterToUsedComponents(ref openApiDocument);

            //Now adjust to context.
            AdjustBasedOnUseCaseAndRole((UseCase) useCase, user.Roles, ref openApiDocument);

            var s = openApiDocument.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Yaml);
            return s;
        }

        private void AdjustBasedOnUseCaseAndRole(UseCase useCase, UserRole[] roles, ref OpenApiDocument openApiDocument)
        {
            switch (useCase)
            {
                case UseCase.Default:
                    return;
                case UseCase.SaxoCreatingClient:
                    MarkRequired("User", "UserName", ref openApiDocument);
                    MarkRequired("User", "Email", ref openApiDocument);
                    MarkRequired("User", "Address", ref openApiDocument);
                    MarkRequired("User", "ZipCode", ref openApiDocument);
                    MarkRequired("User", "IdentificationType", ref openApiDocument);
                    MarkRequired("User", "Roles", ref openApiDocument);
                    LimitEnum("UserRole",
                        new List<string>(){ "RetailUser" }  , 
                        ref openApiDocument);

                    return;
                case UseCase.IBCreatingUser:
                    MarkRequired("User", "UserName", ref openApiDocument);
                    MarkRequired("User", "Email", ref openApiDocument);
                    if (roles== null || roles.Count() == 0) return;
                    if (roles[0]==UserRole.RetailUser) //TODO Check all roles
                    {
                        MarkRequired("User", "UserName", ref openApiDocument);
                        MarkRequired("User", "Email", ref openApiDocument);
                        MarkRequired("User", "Address", ref openApiDocument);
                        MarkRequired("User", "ZipCode", ref openApiDocument);
                        MarkRequired("User", "IdentificationType", ref openApiDocument);
                        MarkRequired("User", "Roles", ref openApiDocument);
                        LimitEnum("UserRole",
                            new List<string>() { "RetailUser" },
                            ref openApiDocument);
                        LimitEnum("IdentificationType",
                         new List<string>() { "Passport" },
                         ref openApiDocument);
                    }
                    else
                    {
                        RemoveProperty("User", "Address",ref openApiDocument);
                        RemoveProperty("User", "ZipCode", ref openApiDocument);
                        RemoveProperty("User", "IdentificationType", ref openApiDocument);
                        LimitEnum("UserRole",
                            new List<string>() { "TradeSupervisor", "TradeManager", "ClientSupervisor", "ClientManager" },
                            ref openApiDocument);
                    }
                    return;
                case UseCase.WLCCreatingClient:
                    RemoveProperty("User", "Email", ref openApiDocument);
                    RemoveProperty("User", "Address", ref openApiDocument);
                    RemoveProperty("User", "ZipCode", ref openApiDocument);
                    RemoveProperty("User", "IdentificationType", ref openApiDocument);
                    LimitEnum("UserRole",
                        new List<string>() { "RetailUser" },
                        ref openApiDocument);
                    return;

                default:
                    throw new NotImplementedException("Not yet");
            }
        }

        /// <summary>
        /// Removes properties from a schema
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="propertyName"></param>
        /// <param name="openApiDocument"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void RemoveProperty(string schemaName, string propertyName, ref OpenApiDocument openApiDocument)
        {
            var schemaEntry = openApiDocument.Components.Schemas.Where(x => x.Key == schemaName).FirstOrDefault();
            var ok = ((OpenApiSchema)schemaEntry.Value).Properties.Remove(propertyName);
        }

        /// <summary>
        /// Removes entries from an enum, if it is not in the list of allowed values
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="allowedValues"></param>
        /// <param name="openApiDocument"></param>
        private void LimitEnum(string schemaName, List<string> allowedValues, ref OpenApiDocument openApiDocument)
        {
            var schemaEntry = openApiDocument.Components.Schemas.Where(x => x.Key == schemaName).FirstOrDefault();
            var enums = ((OpenApiSchema)schemaEntry.Value).Enum;
     
            var itemsToRemove = enums.Where(x => !allowedValues.Contains(((Microsoft.OpenApi.Any.OpenApiString)x).Value)).ToList();
            foreach (var enumItem in itemsToRemove)
                enums.Remove(enumItem);
         }
        /// <summary>
        /// Marks property of a schema to be required.
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="propertyName"></param>
        /// <param name="openApiDocument"></param>
        private void MarkRequired(string schemaName, string propertyName, ref OpenApiDocument openApiDocument)
        {
            var schemaEntry = openApiDocument.Components.Schemas.Where(x => x.Key == schemaName).FirstOrDefault();
            ((OpenApiSchema)schemaEntry.Value).Required.Add(propertyName);
            //var property = ((OpenApiSchema) schemaEntry.Value).Properties.Where(x =>x.Key == propertyName).FirstOrDefault();
            //property.Value.Required. = true;

        }

        /// <summary>
        /// Remove all components which are not referenced by "an" operation
        /// </summary>
        /// <param name="openApiDocument"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void FilterToUsedComponents(ref OpenApiDocument openApiDocument)
        {
            var pathItem = (OpenApiPathItem)openApiDocument.Paths.First().Value;
            var operation = (OpenApiOperation)pathItem.Operations.First().Value;

            //Could consider to also include the response here, for now only looking at the request though
            var requestBody= (OpenApiMediaType) operation.RequestBody.Content.First().Value;
            var schema = requestBody.Schema;

            //Keep track of which components are being used
            var componentList = new Dictionary<string, string>();
            //First look at the request
            WalkSchema(schema, ref componentList);

            //Then find the schema in the component list
            //ToDo I think it is enough to do "once", no need to recurse (I think)
            var components = openApiDocument.Components;
            var componentListKeys = componentList.Keys.ToList();
            foreach (var componentName in componentListKeys)
            {
                var schemaEntry = components.Schemas.Where(x => x.Key == componentName).FirstOrDefault();
                WalkSchema(schemaEntry.Value, ref componentList);
            }
            //Finally remove all entries from the component schema which are not referenced.
            var schemasToRemove = components.Schemas.Where(x => !componentList.ContainsKey(x.Key)).ToList();
            foreach (var schemaEntry in schemasToRemove)
                components.Schemas.Remove(schemaEntry.Key);
        }

        /// <summary>
        /// Creates list of
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="componentList"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void WalkSchema(OpenApiSchema schema, ref Dictionary<string, string> componentList)
        {
            if (schema.Reference != null)
                componentList.TryAdd(schema.Reference.Id, schema.Reference.ReferenceV3);

            if (schema.Items != null)
                WalkSchema(schema.Items, ref componentList);

            if (schema.Properties !=null)
            {
                foreach (var prop in schema.Properties)
                    WalkSchema(prop.Value, ref componentList);
            }
            WalkSchemaList(schema.AllOf, ref componentList);
            WalkSchemaList(schema.OneOf, ref componentList);
            WalkSchemaList(schema.AnyOf, ref componentList);
        }



        private void WalkSchemaList(IList<OpenApiSchema> oasList, ref Dictionary<string, string> componentList)
        {
            if (oasList.Count > 0)
            {
                foreach (var oas in oasList)
                {
                    WalkSchema(oas, ref componentList);
                }
            }
        }


        /// <summary>
        /// Removes all operattions and paths in document, so only the single one for which guidance is sought is included.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="operationType"></param>
        /// <param name="openApiDocument"></param>
        private void FilterToPathAndOperation(string path,OperationType operationType, ref OpenApiDocument openApiDocument)
        {
            var oad = openApiDocument;
            var pathsToRemove = oad.Paths.Where(x => x.Key.ToLower() != path).ToList();
            pathsToRemove.ForEach(x => {oad.Paths.Remove(x.Key); });
            var pathItem = (OpenApiPathItem)oad.Paths.First().Value;
            var operationsToRemove = pathItem.Operations.Where(x => x.Key != operationType).ToList();
            operationsToRemove.ForEach(x => { pathItem.Operations.Remove(x); });
        }

        private OpenApiDocument GetBasicOpenApiDocument()
        {
            var sgo = new SwaggerGeneratorOptions();
            sgo.SwaggerDocs.Add("benny", new OpenApiInfo());

            var schemaGenerator = new SchemaGenerator(new SchemaGeneratorOptions(),
                new JsonSerializerDataContractResolver(new System.Text.Json.JsonSerializerOptions())
                );

            var swg = new SwaggerGenerator(sgo,
                _apiDescriptionGroupCollectionProvider,
                schemaGenerator);
            var openApiDocument = swg.GetSwagger("benny");
            return openApiDocument;
        }


    }
}
