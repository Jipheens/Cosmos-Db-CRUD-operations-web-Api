using FullStack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace EmployeeManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class EmployeeController : ControllerBase
    {
        // Cosmos DB details, In real use cases, these details should be configured in secure configuraion file.
        private readonly string CosmosDBAccountUri = "https://management-db.documents.azure.com:443/";
        private readonly string CosmosDBAccountPrimaryKey = "hY1B6JkQiSesjUyiy8nahz3OAEdgfmgmwejf3zvRzwyjTyVFPTr66Qg2NsnCpoe3VWfdX7WMysIoACDb3IvsXw==";
        private readonly string CosmosDbName = "management.db";
        private readonly string CosmosDbContainerName = "Employees";
        /// <summary>
        /// Commom Container Client, you can also pass the configuration paramter dynamically.
        /// </summary>
        /// <returns> Container Client </returns>
        private Container ContainerClient()
        {
            CosmosClient cosmosDbClient = new CosmosClient(CosmosDBAccountUri, CosmosDBAccountPrimaryKey);
            Container containerClient = cosmosDbClient.GetContainer(CosmosDbName, CosmosDbContainerName);
            return containerClient;
        }
        [HttpPost]
        public async Task<IActionResult> AddEmployee(Employee employee)
        {
            try
            {
                var container = ContainerClient();
                var response = await container.CreateItemAsync(employee, new PartitionKey(employee.HomePhone));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetEmployeeDetails()
        {
            try
            {
                var container = ContainerClient();
                var sqlQuery = "SELECT * FROM c";
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<Employee> queryResultSetIterator = container.GetItemQueryIterator<Employee>(queryDefinition);
                List<Employee> employees = new List<Employee>();
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Employee> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Employee employee in currentResultSet)
                    {
                        employees.Add(employee);
                    }
                }
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetEmployeeDetailsById(string employeeId, string partitionKey)
        {
            try
            {
                var container = ContainerClient();
                ItemResponse<Employee> response = await container.ReadItemAsync<Employee>(employeeId, new PartitionKey(partitionKey));
                return Ok(response.Resource);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        public async Task<IActionResult> UpdateEmployee(Employee emp, string partitionKey)
        {
            try
            {
                var container = ContainerClient();
                ItemResponse<Employee> res = await container.ReadItemAsync<Employee>(emp.id, new PartitionKey(partitionKey));
                //Get Existing Item
                var existingItem = res.Resource;
                //Replace existing item values with new values
                existingItem.id = emp.id;
                existingItem.Country = emp.Country;
                existingItem.City = emp.City;
                existingItem.BirthDate = emp.BirthDate;
                existingItem.HireDate = emp.HireDate;
                existingItem.FirstName = emp.FirstName;
                existingItem.LastName = emp.LastName;
                existingItem.Address = emp.Address;
                existingItem.Title = emp.Title;
                existingItem.PostalCode = emp.PostalCode;
                existingItem.HomePhone = emp.HomePhone;
                var updateRes = await container.ReplaceItemAsync(existingItem, emp.id, new PartitionKey(partitionKey));
                return Ok(updateRes.Resource);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteEmployee(string empId, string partitionKey)
        {
            try
            {
                var container = ContainerClient();
                var response = await container.DeleteItemAsync<Employee>(empId, new PartitionKey(partitionKey));
                return Ok(response.StatusCode);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}