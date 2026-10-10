using GamePlayer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GamePlayer.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options
    ) : base(options)
    {
    }

    public DbSet<PlayerProfile> PlayerProfiles =>
        Set<PlayerProfile>();
    
    public DbSet<Match> Matches =>
        Set<Match>();

    public DbSet<MatchParticipant> MatchParticipants =>
        Set<MatchParticipant>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder
    )
    {
        base.OnModelCreating(modelBuilder);

        var playerProfile =
            modelBuilder.Entity<PlayerProfile>();

        playerProfile.ToTable("player_profiles");

        playerProfile.HasKey(player => player.Id);

        playerProfile
            .Property(player => player.Id)
            .HasColumnName("id");

        playerProfile
            .Property(player => player.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(30)
            .IsRequired();

        playerProfile
            .Property(player => player.Region)
            .HasColumnName("region")
            .HasMaxLength(30)
            .IsRequired();

        playerProfile
            .Property(player => player.Bio)
            .HasColumnName("bio")
            .HasMaxLength(200);

        playerProfile
            .Property(player => player.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        playerProfile
            .Property(player => player.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        playerProfile
            .HasIndex(player => player.DisplayName)
            .IsUnique();


        var match =
            modelBuilder.Entity<Match>();

        match.ToTable("matches");

        match.HasKey(match => match.Id);

        match
            .Property(match => match.Id)
            .HasColumnName("id");

        match
            .Property(match => match.ExternalMatchId)
            .HasColumnName("external_match_id")
            .HasMaxLength(100)
            .IsRequired();

        match
            .Property(match => match.GameMode)
            .HasColumnName("game_mode")
            .HasMaxLength(30)
            .IsRequired();

        match
            .Property(match => match.PlayedAtUtc)
            .HasColumnName("played_at_utc")
            .IsRequired();

        match
            .Property(match => match.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        match
            .HasIndex(match => match.ExternalMatchId)
            .IsUnique();


        var matchParticipant =
            modelBuilder.Entity<MatchParticipant>();

        matchParticipant.ToTable("match_participants");

        matchParticipant.HasKey(participant => participant.Id);

        matchParticipant
            .Property(participant => participant.Id)
            .HasColumnName("id");

        matchParticipant
            .Property(participant => participant.MatchId)
            .HasColumnName("match_id");

        matchParticipant
            .Property(participant => participant.PlayerProfileId)
            .HasColumnName("player_profile_id");

        matchParticipant
            .Property(participant => participant.Score)
            .HasColumnName("score");

        matchParticipant
            .Property(participant => participant.Kills)
            .HasColumnName("kills");

        matchParticipant
            .Property(participant => participant.Deaths)
            .HasColumnName("deaths");

        matchParticipant
            .Property(participant => participant.Result)
            .HasColumnName("result")
            .HasMaxLength(10)
            .IsRequired();

        matchParticipant
            .HasOne(participant => participant.Match)
            .WithMany(match => match.Participants)
            .HasForeignKey(participant => participant.MatchId)
            .OnDelete(DeleteBehavior.Cascade);
        
        matchParticipant
            .HasOne(participant => participant.PlayerProfile)
            .WithMany(player => player.MatchParticipants)
            .HasForeignKey(participant => participant.PlayerProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        
        matchParticipant
            .HasIndex(
                participant => new
                {
                    participant.MatchId,
                    participant.PlayerProfileId
                }
            )
            .IsUnique();
    }
}