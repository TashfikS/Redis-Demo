# Redis-Demo

## Project Pre-requisite

In Visual Studio: Goto - Tools -> NuGet Package Manager -> Package Manager Console

### Migration 
`Add-Migration <MigrationName>`

### Update Database 
`Update-Database`

## Redis Installation in Docker

Docker Command: docker run -d --name redis-container -p 6379:6379 -v redis-data:/data redis:latest

    -d: Runs the container in the background (detached mode).
    --name redis-container: Assigns a custom name to the container.
    -p 6379:6379: Maps the host machine's port 6379 to the container's port 6379.
    -v redis-data:/data: Creates a volume named redis-data and mounts it to the /data directory in the container.
    redis:latest: Specifies the Redis Docker image and its version (latest).

## Redis Setup in Project 

### In appsettings.json

`"ConnectionStrings": {
  "Redis": "localhost:6379"
}`

### In Program.cs

`builder.Services.AddSingleton<IConnectionMultiplexer>(c =>
{
    var options = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"));
    return ConnectionMultiplexer.Connect(options);
});`

## Redis Connection in Project

### In service class

`public CacheService(IConnectionMultiplexer redis)
{
    _cacheDb = redis.GetDatabase();
}`

## Redis Operations

### Get Data

`public T GetData<T>(string key)
{
    var value = _cacheDb.StringGet(key);

    if (!string.IsNullOrEmpty(value))
        return JsonSerializer.Deserialize<T>(value);

    return default;
}`

### Set Data 

`public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
{
    var expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);
    return _cacheDb.StringSet(key, JsonSerializer.Serialize(value), expiryTime);
}`

### Remove Data 

`public object RemoveData(string key)
{
    var _exist = _cacheDb.KeyExists(key);

    if (_exist)
        return _cacheDb.KeyDelete(key);

    return false;
}`

## Redis Implementation in Controller

### Get Cache Data
`        public async Task<ActionResult> Get()
        {
            // check cache data
            var cacheData = _cacheService.GetData<IEnumerable<Driver>>("<Key>");

            if (cacheData != null && cacheData.Count() > 0)
                return Ok(cacheData);

            // Save in Relational Database

            // set expiry time
            var expiryTime = DateTimeOffset.Now.AddSeconds(30);

            // If data is not exist in cache, then set the data
            _cacheService.SetData<IEnumerable<Driver>>("drivers", DataFromRelationalDB, expiryTime);

            return Ok(cacheData);
        }`

### Set Cache Data
`        [HttpPost("AddDrivers")]
        public async Task<ActionResult> Post(<Model> value)
        {
            // Add into Relational Database

            // set expiry time
            var expiryTime = DateTimeOffset.Now.AddSeconds(30);

            // set in the cache
            _cacheService.SetData<Driver>($"driver{value.Id}", DBInstanceOfValue.Entity, expiryTime);

            // Save in Relational Database 

            return Ok(DBInstanceOfValue.Entity);
        }`

### Remove Cache Data
`        public async Task<ActionResult> Delete(int id)
        {
            // get the existing value from Relational DB

            if (exist != null)
            {
                // Remove from Relational DB
                _cacheService.RemoveData($"driver{id}");
                // Save in Relational Database 

                return NoContent();
            }

            return NotFound();
        }`

## To check the cache data

### Pre-requisite

#### In terminal - 

##### Find the container ID or name
`docker ps`

#####  Access the Redis container's shell
`docker exec -it <container_id_or_name> sh`

#####  Once inside the container, you can use the redis-cli tool to interact with Redis
`redis-cli`

### Checking

##### List all keys:
`KEYS *`

#### Get the value of a specific key:
`GET <key_name>`

#### Set a key-value pair:
`SET <key_name> <value>`

#### Delete a key:
`DEL <key_name>`

#### Check if a key exists:
`EXISTS <key_name>`

#### Get information about the Redis server:
`INFO`

#### Exit Redis CLI:
`exit`

# Distributed Caching Concept
[Concept Site Link](https://medium.com/@sudheer.sandu/distributed-caching-the-only-guide-youll-ever-need-fe152357f912)
